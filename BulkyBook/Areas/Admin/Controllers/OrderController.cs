using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Repository.IRepository;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public  OrderDetailsVM OrderVM { get; set;}



        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }




        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public IActionResult OrderDetails(int id)
        {
            OrderVM = new OrderDetailsVM
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id,
                               includeProperties: "ApplicationUser"),

                OrderDetails = _unitOfWork.OrderDetails.GetAll(u=>u.OrderId==id,includeProperties:"Product")
            };

            return View(OrderVM);
        }

        [Authorize(Roles =SD.Role_Admin +","+ SD.Role_Employee)]
        public IActionResult StartProcessing(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u=>u.Id == id);
            orderHeader.OrderStatus = SD.StatusInProcess;
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }




        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            _unitOfWork.Save();
            return RedirectToAction("Index");

        }





        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);

            //Means Refund the money to customer if the money is recieved and product have not shipped 
            if (orderHeader.OrderStatus== SD.StatusApproved)
            {
                var options = new RefundCreateOptions()
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal*100),
                    Reason = RefundReasons.RequestedByCustomer,
                    Charge= orderHeader.TransactionId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                orderHeader.OrderStatus = SD.StatusRefunded;
                orderHeader.PaymentStatus = SD.StatusRefunded;
            }
            else
            {
                orderHeader.OrderStatus = SD.StatusCancelled;
                orderHeader.PaymentStatus = SD.StatusCancelled;
            }

            _unitOfWork.Save();
            return RedirectToAction("Index");

        }




        //Recieving Payment from Authorized User After Shipping order
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("OrderDetails")]
        public IActionResult Details(string stripeToken)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id,
                                                includeProperties: "ApplicationUser");
            if (stripeToken != null)
            {
                //process the payment
                var options = new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal * 100),
                    Currency = "usd",
                    Description = "Order ID : " + orderHeader.Id,
                    Source = stripeToken
                };

                var service = new ChargeService();
                Charge charge = service.Create(options);

                if (charge.Id == null)
                {
                    orderHeader.PaymentStatus = SD.PaymentStatusRejected;
                }
                else
                {
                    orderHeader.TransactionId = charge.Id;
                }
                if (charge.Status.ToLower() == "succeeded")
                {
                    orderHeader.PaymentStatus = SD.PaymentStatusApproved;

                    orderHeader.PaymentDate = DateTime.Now;
                }

                _unitOfWork.Save();

            }
            return RedirectToAction("OrderDetails", "Order", new { id = orderHeader.Id });
        }




        #region API CALLS

        [HttpGet]
        public IActionResult GetOrderList(string status)
        {
           

            IEnumerable<OrderHeader> orderHeaderList;

            var UserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            //Getting all order if the User is admin or Employee
            if (User.IsInRole(SD.Role_Admin) || HttpContext.User.IsInRole(SD.Role_Employee))
            {
                orderHeaderList = _unitOfWork.OrderHeader.GetAll(includeProperties:"ApplicationUser");
            }
            //else fetch only records of the loged in user
            else
            {
                orderHeaderList = _unitOfWork.OrderHeader.GetAll(
                                      u => u.ApplicationUserId == UserId,
                                      includeProperties: "ApplicationUser");
            }


            switch (status)
            {
                case "pending":
                    orderHeaderList = orderHeaderList.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;

                case "completed":
                    orderHeaderList = orderHeaderList.Where(o => o.OrderStatus == SD.StatusApproved ||
                                                            o.OrderStatus == SD.StatusInProcess ||
                                                            o.OrderStatus == SD.StatusPending);
                    break;

                case "rejected":
                    orderHeaderList = orderHeaderList.Where(o => o.OrderStatus == SD.StatusCancelled ||
                                                            o.OrderStatus == SD.StatusRefunded ||
                                                            o.OrderStatus == SD.PaymentStatusRejected);
                    break;

                default:
                    break;
            }
           


            return Json(new { data = orderHeaderList });
        }


        #endregion
    }
}

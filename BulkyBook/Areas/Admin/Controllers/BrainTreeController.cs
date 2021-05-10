﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Braintree;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BrainTreeController : Controller
    {
        public readonly IBrainTreeGate _brain;
        public BrainTreeController(IBrainTreeGate brain)
        {
            _brain = brain;
        }

        public IActionResult Index()
        {
            var getway = _brain.GetGateway();
            //Generating thr client token for authorization
            var clientToken = getway.ClientToken.Generate();
            ViewBag.ClientToken = clientToken;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(IFormCollection collection)
        {
            Random rnd = new Random();
            //Collecting the nonce from brain tree
            string nonceFromtheClient = collection["payment_method_nonce"];
            //requesr object
            var request = new TransactionRequest
            {
                Amount = rnd.Next(1, 100),
                PaymentMethodNonce = nonceFromtheClient,
                OrderId = "55501",
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                }
            };

            var gateway = _brain.GetGateway();
            Result<Transaction> result = gateway.Transaction.Sale(request);

            if (result.Target.ProcessorResponseText == "Approved")
            {
                TempData["Success"] = "Transaction was successful Transaction ID "
                                + result.Target.Id + ", Amount Charged : $" + result.Target.Amount;
            }
            return RedirectToAction("Index");
        }
    }
}

var datatable;

$(document).ready(function () {

    //Window.Location.search will get the query string from the url : ?Status=inprocess 
    var url = window.location.search;

    //Include method check for the given string 
    if (url.includes("inprocess")) {
        loadDataTable("GetOrderList?status=inprocess");
    }
    else {
        if (url.includes("pending")) {
            loadDataTable("GetOrderList?status=pending");
        }
        else {
            if (url.includes("completed")) {
                loadDataTable("GetOrderList?status=completed");
            }
            else {
                if (url.includes("rejected")) {
                    loadDataTable("GetOrderList?status=rejected");
                }
                else {
                    loadDataTable("GetOrderList?status=all");
                }
            }
        }
    }
 
});

function loadDataTable(url) {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Order/" + url
        },
        "columns": [
            { "data": "id", "width": "10%" },
            { "data": "name", "width": "15%" },
            { "data": "phoneNumber", "width": "15%" },
            { "data": "applicationUser.email", "width": "15%" },
            { "data": "orderStatus", "width": "15%" },
            { "data": "orderTotal", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                            <div class="text-center">
                                <a href="/Admin/Order/OrderDetails/${data}" class="btn btn-success text-white" style="cursor:pointer">
                                    <i class="fas fa-edit"></i> 
                                </a>
                            </div>
                           `;
                }, "width": "5%"
            }
        ]
    });
}



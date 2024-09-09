$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Product/GetAll",
            "error": function (jqXHR, textStatus, errorThrown) {
                toastr.error("Failed to load data: " + textStatus);
            }
        },
        "columns": [
            { "data": "productCode", "width": "15%" },
            { "data": "category.name", "width": "15%" }, 
            { "data": "price", "width": "5%" },
            { "data": "username", "width": "20%" },
            {
                "data": "createdAt",
                "width": "15%",
                "render": function (data) {
                    return formatDate(data) || "N/A";
                }
            },
            {
                "data": "updateAt",
                "width": "20%",
                "render": function (data) {
                    return data ? formatDate(data) : "N/A";
                }
            },
            {
                "data": "productId",
                "render": function (data) {
                    return `
                        <div class="w-75 btn-group" role="group">
                        <a href="/Admin/Product/Upsert?id=${data}"
                        class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Edit</a>
                        <a onClick="Delete(${data})"
                        class="btn btn-danger mx-2"> <i class="bi bi-trash-fill"></i> Delete</a>
                    </div>
                    `;
                },
                "width": "5%"
            }
        ]
    });
}

function formatDate(dateString) {
    if (!dateString) return null;
    var date = new Date(dateString);
    var options = { year: 'numeric', month: 'short', day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit' };
    return date.toLocaleDateString('en-GB', options);
}

function Delete(productId) {
    $.ajax({
        url: `/Admin/Product/Delete/${productId}`,
        type: 'DELETE',
        success: function (data) {
            if (data.success) {
                dataTable.ajax.reload();
                debugger
                //toastr.success(data.message);
            } else {
                //toastr.error(data.message);
            }
        },
        error: function (xhr, status, error) {
            //toastr.error("Delete failed: " + error);
        }
    });
}

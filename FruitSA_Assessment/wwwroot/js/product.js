$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Product/GetAll"
        },
        "columns": [
            { "data": "productName", "width": "20%" },
            { "data": "productCode", "width": "15%" },
            //{ "data": "category.categoryName", "width": "15%" },
            { "data": "price", "width": "5%" },
            { "data": "username", "width": "20%" },
            {
                "data": "createdAt",
                "width": "15%",
                "render": function (data) {
                    return formatDate(data);
                }
            },
            {
                "data": "updateAt",
                "width": "20%",
                "render": function (data) {
                    if (data) {
                        return formatDate(data);
                    } else {
                        return "N/A";
                    }
                }
            },
            {
                "data": "productId",
                "render": function (data) {
                    return `
                        <div class="w-75 btn-group" role="group">
                        <a href="/Admin/Product/Upsert?id=${data}"
                        class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Edit</a>
                        <a onClick=Delete('/Admin/Product/Delete/${data}')
                        class="btn btn-danger mx-2"> <i class="bi bi-trash-fill"></i> Delete</a>
					</div>
                        `
                },
                "width": "5%"
            }
        ]
    });
}


function formatDate(dateString) {
    if (!dateString) return "";
    var date = new Date(dateString);
    var options = { year: 'numeric', month: 'short', day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit' };
    return date.toLocaleDateString('en-GB', options);
}

function Delete(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload();
                        toastr.success(data.message);
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}

var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        ajax: { url: '/Admin/Product/getall' },

        columns: [
            { data: 'brand', "width": "7.5%" },
            { data: 'shoeModel', "width": "7.5%" },
            { data: 'listPrice', "width": "5%" },
            { data: 'discount',  "width": "5%" },
            { data: 'purchasesCount', "width": "5%" },
            { data: 'stockCount', "width": "5%" },
            { data: 'stockStatus', "width": "5%" },
            { data: 'category.name', "width": "5%" },
            { data: 'gender', "width": "5%" },
            { data: 'ageGroup', "width": "5%" },
            
            {
                data: 'id',
                "render": function (data) {
                    return `
                    <div class="btn-group" role="group" style="margin: 0 auto;">
                        <a href="/Admin/Product/Upsert/${data}" class="btn btn-primary text-white table_icon_btn">
                            <i class="bi bi-pencil" style="margin-right: 3px;"></i> Edit </a>
                        &nbsp;
                        <a onClick=Delete('/Admin/Product/Delete/${data}') class="btn btn-danger text-white table_icon_btn">
                            <i class="bi bi-trash" style="margin-right: 2px;"></i> Delete </a>
                            &nbsp;
                        <a href="/Admin/Product/Warehouse/${data}" class="btn btn-success text-white table_icon_btn">
                            <i class="bi bi-archive" style="margin-right: 6px;"></i> Warehouse </a>
                    </div>
                    `;
                },
                "width": "35%"
            }
        ]
    });
}

function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: "DELETE",
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dataTable.ajax.reload();
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    });
}
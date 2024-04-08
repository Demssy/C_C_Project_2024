
var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        ajax: { url: '/Admin/Product/getall' },

        columns: [
            { data: 'brand', "width": "10%" },
            { data: 'shoeModel', "width": "10%" },
            { data: 'size', "width": "5%" },
            { data: 'listPrice', "width": "5%" },
            { data: 'category.name', "width": "10%" },
            { data: 'stockStatus', "width": "10%" },
            { data: 'gender', "width": "5%" },
            { data: 'stockCount', "width": "5%" },
            { data: 'ageGroup', "width": "5%" },
            {
                data: 'id',
                "render": function (data) {
                    return `
                    <div class="w-75 btn-group" role="group">
                        <a href="/Admin/Product/Upsert/${data}" class="btn btn-primary mx-2 text-white" style="cursor:pointer; width:110px; height:60px;">
                            <i class="bi bi-pencil"></i> Edit </a>
                        &nbsp;
                        <a onClick=Delete('/Admin/Product/Delete/${data}') class="btn btn-danger text-white" style="cursor:pointer; width:110px; margin-left:20px; height:60px;">
                            <i class="bi bi-trash"></i> Delete </a>
                            &nbsp;
                        <a href="/Admin/Product/Warehouse/${data}" class="btn btn-success text-white " style="cursor:pointer; width:110px; margin-left:30px; height:60px;">
                            <i class="bi bi-archive"></i> Warehouse </a>
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
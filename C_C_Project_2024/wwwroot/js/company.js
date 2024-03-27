
var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        ajax: { url: '/Admin/Company/getall' },

        columns: [
            { data: 'name', "width": "15%" },
            { data: 'country', "width": "15%" },
            { data: 'city', "width": "15%" },
            { data: 'phoneNumber', "width": "15%" },
            { data: 'email', "width": "15%" },
            {
                data: 'id',
                "render": function (data) {
                    return `
                    <div class="w-75 btn-group" role="group">
                        <a href="/Admin/Company/Upsert/${data}" class="btn btn-success text-white" style="cursor:pointer; width:100px;">
                            <i class="bi bi-pencil"></i> Edit </a>
                        &nbsp;
                        <a onClick=Delete('/Admin/Company/Delete/${data}') class="btn btn-danger text-white" style="cursor:pointer; width:100px;">
                            <i class="bi bi-trash"></i> Delete </a>
                    </div>
                    `;
                },
                "width": "25%"
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
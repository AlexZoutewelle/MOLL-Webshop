// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.

function AddToCookieCart(id) {
    var fetch_data = '';
    $.ajax({
        type: "GET",
        contentType: "html",
        async: false,
        data: { fld_offeredserviceid: id },
        url: '../Cookie/AddToShoppingCart',
        succes: function (result) {
            fetch_data = result;
        }
    })
    return fetch_data;
};

function AddToDbCart(id, userId) {
    var fetch_data = ''
    $.ajax({
        type: "GET",
        contentType: "html",
        async: false,
        data: { fld_offeredserviceid: id , fld_userid : userId},
        url: "../ShoppingCart/AddToDbShoppingCart",
        succes: function (result) {
            fetch_data = result;
        }
    })
    return fetch_data;
};


function CreateOrders(orderMail) {
    $.ajax({
        type: "GET",
        contentType: "html",
        async: false,
        data: {
            fld_email: orderMail
        },
        url: '/ShoppingCart/CreateOrders',
        succes: function (result) {
            alert("Order saved!");
        }
    })
};


$(document).ready(function () {

    $('#btnSearch').click(function () {
        var Area = $('#Area').val();
        var LowerCost = $('#LowerCost').val();
        var HigherCost = $('#HigherCost').val();
        var TimeAvailableBegin = $('#TimeAvailableBegin').val();
        var TimeAvailableEnd = $('#TimeAvailableEnd').val();
        var Category = $('#Category').val();
        var ServiceName = $('#ServiceName').val();
        var Gender = $('#Gender').val();
        var FirstName = $('#FirstName').val();
        var LastName = $('#LastName').val();
        var StillAvailable = $('#StillAvailable').val();
        $.ajax({
            type: 'GET',
            data: {
                fld_area: Area, lowerCost: LowerCost, higherCost: HigherCost, fld_timefirst: TimeAvailableBegin,
                fld_timelast: TimeAvailableEnd, fld_category: Category, fld_name: ServiceName,
                fld_gender: Gender, fld_firstname: FirstName, fld_lastname: LastName,
                fld_stillavailable : StillAvailable

            },
            url: '/Search/SearchResult',
            success: function (result) {
                $('#SearchResult').replaceWith(result);
            }
        });
    });
});







//Admin CRUD


//Row selection: Manage users
$('#tbl_manageusers').on('click', "tr", function () {
    var selected = $(this).hasClass("highlight");
    $("#tbl_manageusers .clickableRow").removeClass("highlight");
    if (!selected) {
        $(this).addClass("highlight");
    };
});

$('#tbl_manageusers').on('page.dt', function () {
    $("#tbl_manageusers .clickableRow").removeClass("highlight");
    console.log('page change');
});

$('#tbl_manageusers').on('search.dt', function () {
    $("#tbl_manageusers .clickableRow").removeClass("highlight");
    console.log('filtering event');
});

//Row selection: Manage services

$('#tbl_manageservices').on('click', "tr", function () {
    var selected = $(this).hasClass("highlight");
    $("#tbl_manageservices .clickableRow").removeClass("highlight");
    if (!selected) {
        $(this).addClass("highlight");
    };
});

$('#tbl_manageservices').on('page.dt', function () {
    $("#tbl_manageservices .clickableRow").removeClass("highlight");
    console.log('page change');
});

$('#tbl_manageservices').on('search.dt', function () {
    $("#tbl_manageservices .clickableRow").removeClass("highlight");
    console.log('filtering event');
});

//Row selection: Manage labourers

$('#tbl_managelabourers').on('click', "tr", function () {
    var selected = $(this).hasClass("highlight");
    $("#tbl_managelabourers .clickableRow").removeClass("highlight");
    if (!selected) {
        $(this).addClass("highlight");
    };
});

$('#tbl_managelabourers').on('page.dt', function () {
    $(".clickableRow").removeClass("highlight");
    console.log('page change');
});

$('#tbl_managelabourers').on('search.dt', function () {
    $("#tbl_managelabourers .clickableRow").removeClass("highlight");
    console.log('filtering event');
});


//Deletion



$(document).ready(function () {
    $("#btnDelete").click(function () {
        var id = $(this).attr('id');
        console.log(id);
        $("#ModalDelete").modal('show');
    });

    $("#btnDeleteClose").click(function () {
        var id = $(this).attr('id');
        $("#ModalDelete").modal('hide');
    });
});


function checkForDependencies(itemId, objectType) {
    var foundItems = 0;
    $.ajax({
        type: 'GET',
        data: { id: itemId, type: objectType },
        url: '/Admin/checkForDependency',
        async: false,
        success: function (result) {
            foundItems = result;
        }
    });

    return foundItems;
}

function deleteFromDatabase(itemId, objectType) {
    $.ajax({
        type: 'GET',
        data: { id: itemId, type: objectType },
        url: '/Admin/DeleteItem',
        success: function (result) {
            alert('Deletion successful!')
        }
    });
}

//Delete the row from the table, return the item's ID on success
function deleteFromTable(tableSelector) {
    var row = $(tableSelector).DataTable()
        .rows('.highlight');

    var data = row.data();

    //Check if there is an item selected
    if (data.length === 0) {
        return -1;
    }

    var itemId = data[0][0];

    //remove from table
    row.remove().draw();

    //return id
    return itemId
} 

//Get the item's ID without deleting the row from the datatable
function getRowItemId(tableSelector) {
    var row = $(tableSelector).DataTable()
        .rows('.highlight');

    var data = row.data();

    //Check if there is an item selected
    if (data.length === 0) {
        return -1;
    }

    var itemId = data[0][0];

    return itemId;
}


//Creation

//Opening the Modal
$(document).ready(function () {
    $("#btnCreate").click(function () {
        var id = $(this).attr('id');
        console.log(id);
        $("#ModalCreate").modal('show');
    });

    $("#btnCreateClose").click(function () {
        var id = $(this).attr('id');
        $("#ModalCreate").modal('hide');
    });
});

//Initialize the creation process
function InitializeCreation(tableSelector) {
    //The array for the database
    indexed_array = createIndexedArrayFromForm("#newItemForm");

    //The array for the table
    dataObj = createDataObjectFromForm("#newItemForm");

    console.log(dataObj);

    //Hidden input is the object type we need to pass on
    var objectType = $('#objectType').val();

    //Insert the item and get its ID
    var newId = insertItemIntoDatabase(indexed_array, objectType);

    if (newId > 0) {
        //Successful creation
        alert('Creation was successful!')

        dataObj[0] = newId;

        console.log(dataObj);

        $(tableSelector).DataTable().row.add(dataObj);

        $(tableSelector).DataTable().draw();
    }

    if (newId === -1) {
        alert('This email address has already been taken!')
    }

}


//Create an Indexed Array that's to be inserted in our databases
function createIndexedArrayFromForm(formSelector) {
    var dataArray = $(formSelector).serializeArray();

    //Index the array, like a Dictionary, so we can later use it in a .NET controller
    console.log(' data array');
    console.log(dataArray);
    var indexed_array = {};

    $.map(dataArray, function (n, i) {
        indexed_array[n['name']] = n['value'];
    });

    return indexed_array;
}

//Create a new row that's to be inserted into the Admin Datatable
function createDataObjectFromForm(formSelector) {
    var dataArray = $(formSelector).serializeArray();
    var dataObj = {};
    var counter = 0;
    //Make space for the ID, as it must be generated from the database
    $(dataArray).each(function (i, field) {
        counter = counter + 1;
        if (field.name == 'fld_password') {
            counter = counter - 1;

        }
        else {
            dataObj[counter] = field.value;
        }
    });

    return dataObj;
}
//Insert the new Item into the database
function insertItemIntoDatabase(indexed_array, objectType) {
    var newId = 0;
    $.ajax({
        type: 'GET',
        async: false,
        data: { insertedDic: JSON.stringify(indexed_array), type: objectType },
        url: '/Admin/CreateItem',
        success: function (result) {

            //We return the generated ID
            newId = result;

        }
    });

    return newId;
}

//Updating

//Opening the Modal
$(document).ready(function () {
    $(".btnEdit").click(function () {

        //Selecting the table
        var tableSelector = $(this).attr('id');
        console.log(tableSelector);


        //Getting the highlighted rows
        var row = $(tableSelector).DataTable()
            .rows('.highlight');

        //Converting the row to a javascript array
        var data = row.data().toArray();

        //If there are 0 items in the array, nothing has been selected, and we must return
        if (data.length === 0) {
            alert('No row selected!');
            return;
        }


        //Populating the Edit form
        var inputSelectorStart = '#editInput';
        var counter = 1;
        for (var i = 1; i < data[0].length; i++) {

            var inputSelector = inputSelectorStart + counter;

            console.log(inputSelector);

            $(inputSelector).attr('value', data[0][i]);

            counter = counter + 1;
        }

        //If we're dealing with services or labourers, we render a preview
        if (tableSelector === '#tbl_manageservices' || tableSelector === '#tbl_managelabourers') {

            RenderPreviewForUpdate();
        }

        $("#ModalEdit").modal('show');
    });

    $("#btnEditClose").click(function () {
        var id = $(this).attr('id');
        $("#ModalEdit").modal('hide');
    });
});

//Rendering previews for updates, the button
$(document).ready(function () {
    $('#btnRenderUpdatePreview').click(function () {
        RenderPreviewForUpdate();
    });
});

//Rendering previews for creations, the button
$(document).ready(function () {
    $('#btnRenderCreationPreview').click(function () {
        RenderPreviewForCreation();
    });
});


//The function for preview rendering
function RenderPreviewForUpdate() {

    //Turn the form's data into the object to preview
    objectToPreview = createIndexedArrayFromForm("#updateItemForm");
    var objectType = $('#objectType').val();


    var previewHTML = FetchPreviewHTML(objectToPreview, objectType);

    $('.UpdateItemPreviewDiv').html(previewHTML);
}

function RenderPreviewForCreation() {

    //Turn the form's data into the object to preview
    objectToPreview = createIndexedArrayFromForm("#newItemForm");
    var objectType = $('#objectType').val();

    var previewHTML = FetchPreviewHTML(objectToPreview, objectType);

    console.log('rendering...');
    $('.CreationItemPreviewDiv').html(previewHTML);

}

//Deze functie moet zijn result returnen, zodat we het kunnen gebruiken in de RenderPreviewFor... functies
function FetchPreviewHTML(objectToPreview, objectType) {
    var html = '';
    $.ajax({
        type: 'GET',
        async: false,
        contentType: 'html',
        data: {
            insertedDic: JSON.stringify(objectToPreview), type: objectType
        },
        url: '/Admin/ItemPreview',
        success: function (result) {
            console.log('success!');
            html = result;
            return html;
        }
    });
    return html;
}

//Initialize the Update process
function InitializeUpdate(tableSelector) {

    //Get the row and the item's ID
    var row = $(tableSelector).DataTable().row('.highlight').data();
    var rowId = row['DT_RowId'];
    console.log(row);
    console.log('the id: ' + rowId);

    var objectType = $('#objectType').val();
    console.log('object type: ' + objectType);


    if (objectType === "offeredlabourerservice") {
        var OLS = UpdateDatabaseOLS(rowId);
        var dataObj = CreateTableOLSFromObject(OLS);
        $(tableSelector).DataTable().row('.highlight').data(dataObj).invalidate();

        alert('Update successfull!');
        return;
    }

    else {
        var result = UpdateDatabase(rowId, objectType);

    }

    if (result === -1) {
        alert('Failed to Update the item');
    }

    if (result === 1) {
        UpdateTable(tableSelector, rowId);
        alert('Update successfull!');

    }
}


function UpdateTable(tableSelector, rowId) {
    //The array for the table
    dataObj = createDataObjectFromForm("#updateItemForm");
    dataObj[0] = rowId;

    console.log('update table')
    console.log(dataObj);

    $(tableSelector).DataTable().row('.highlight').data(dataObj).invalidate();
}



function UpdateDatabase(rowId, objectType) {
    //The array for the database
    indexed_array = createIndexedArrayFromForm("#updateItemForm");
    if (objectType === "offeredlabourerservice") {
        indexed_array["fld_offeredserviceid"] = rowId;
    }

    else {
        var idField = objectType.replace('tbl_', 'fld_');
        idField = idField.replace('data', 'id');


        indexed_array[idField] = rowId;
    }

    console.log('update database');

    var resultString = '';

    $.ajax({
        type: 'GET',
        async: false,
        data: { insertedDic: JSON.stringify(indexed_array), type: objectType },
        url: '/Admin/EditItem',
        success: function (result) {

            //We return the result

            resultString = result;
        }
    });

    return resultString;
}




//Manage OLS

$('#tbl_manageOLS').on('click', "tr", function () {
    var selected = $(this).hasClass("highlight");
    $("#tbl_manageOLS .clickableRow").removeClass("highlight");
    if (!selected) {
        $(this).addClass("highlight");
    };
});

$('#tbl_manageOLS').on('page.dt', function () {
    $("#tbl_manageOLS .clickableRow").removeClass("highlight");
    console.log('page change');
});

$('#tbl_manageOLS').on('search.dt', function () {
    $("#tbl_manageOLS .clickableRow").removeClass("highlight");
    console.log('filtering event');
});


//This function retrieves the html that goes into the Creation Modal.
function GetCreationModal() {
    var html = '..Loading..';
    $.ajax({
        type: 'GET',
        async: false,
        contentType: 'html',
        url: '/Admin/OLSCreationDiv',
        success: function (result) {
            html = result;
            return html;

        }
    });
    return html;
}


//OLS editing

function UpdateDatabaseOLS(rowId) {
    //The array for the database
    indexed_array = createIndexedArrayFromForm("#updateItemForm");

    indexed_array["fld_offeredserviceid"] = rowId;

    var resultString = '';

    $.ajax({
        type: 'GET',
        async: false,
        data: { insertedDic: JSON.stringify(indexed_array) },
        url: '/Admin/UpdateOLS',
        success: function (result) {

            //We return the result

            resultString = result;
        }
    });

    return resultString;
}

function CreateTableOLSFromObject(object) {

    var dataObj = {};
    var counter = 0;
    console.log('begin conversion');

    var dataObj = {};
    var counter = 0;
    for (var property in object) {
        dataObj[counter] = object[property];
        counter = counter + 1;
    }

    return dataObj;
}


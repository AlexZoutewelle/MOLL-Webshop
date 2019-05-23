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

//Row selection: Manage labourers

$('#tbl_managelabourers').on('click', "tr", function () {
    var selected = $(this).hasClass("highlight");
    $("#tbl_managelabourers .clickableRow").removeClass("highlight");
    if (!selected) {
        $(this).addClass("highlight");
    };
});

$('#tbl_managelabourers').on('page.dt', function () {
    $("#tbl_managelabourers .clickableRow").removeClass("highlight");
    console.log('page change');
});



//Creation

$('.olscreationclose').click(function () {
    $('.clickableRow').removeClass('highlight');
    $('.creationDiv').html('');
});

$('#btnCreateNewOLS').click(function () {

    var creationArray = GetCreationArray();

    var CreationResult = CreateNewOLS(creationArray[0], creationArray[1], creationArray[2]);

    if (CreationResult != undefined) {
        alert("Labourer has successfully been linked to the Service!");
        //Update de datatable

        var dataObj = CreateTableObjectFromObject(CreationResult);
        console.log('CreationResult:');
        console.log(CreationResult);
        console.log('dataObject');
        console.log(dataObj);

        $('#tbl_manageOLS').DataTable().row.add(dataObj);

        $('#tbl_manageOLS').DataTable().draw();

        return;


    }
    else {
        alert('Something went wrong while creating your new offer..');
    }

});

$('#btnRenderOLSCreationPreview').click(function () {

    $('.CreationItemPreviewDiv').html('Click the Render Preview button to preview the Offered service!');

    var previewArray = GetCreationArray();

    var previewHTML = FetchOLSPreviewHTML(previewArray[0], previewArray[1], previewArray[2]);

    $('.CreationItemPreviewDiv').html(previewHTML);

});

function OLSPreviewRender() {
    $('.CreationItemPreviewDiv').html('Click the Render Preview button to preview the Offered service!');

    var previewArray = GetCreationArray();

    var previewHTML = FetchOLSPreviewHTML(previewArray[0], previewArray[1], previewArray[2]);

    $('.CreationItemPreviewDiv').html(previewHTML);
}

function CreateTableObjectFromObject(object) {

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





function GetCreationArray() {

    var serviceId = GetServiceId();
    var labourerId = GetLabourerId();

    if (labourerId === undefined) {
        alert('You have not selected a labourer!');
        return;
    }

    if (serviceId === undefined) {
        alert('You have not selected a service!');
        return;
    }

    indexed_array = createIndexedArrayFromForm("#newItemForm");

    var results = [indexed_array, serviceId, labourerId];

    return results;
}

function GetServiceId() {
    //Get the selected service
    var service = $('#tbl_manageservices').DataTable()
        .rows('.highlight');

    //Converting the row to a javascript array
    var servicedata = service.data().toArray();


    //If there are 0 items in the array, nothing has been selected, and we must return
    if (servicedata.length === 0) {
        return;
    }

    return servicedata[0][0];
}

function GetLabourerId() {
    //Get the selected labourer
    var labourer = $('#tbl_managelabourers').DataTable()
        .rows('.highlight');

    //Converting the row to a javascript array
    var labourerdata = labourer.data().toArray();

    //If there are 0 items in the array, nothing has been selected, and we must return
    if (labourerdata.length === 0) {
        return;
    }

    return labourerdata[0][0];
}

//Deze functie moet zijn result returnen, zodat we het kunnen gebruiken in de RenderPreviewFor... functies
function FetchOLSPreviewHTML(objectToPreview, servId, labId) {
    var html = '';
    $.ajax({
        type: 'GET',
        async: false,
        contentType: 'html',
        data: {
            offeredService: JSON.stringify(objectToPreview), serviceId: servId, labourerId: labId
        },
        url: '/Admin/OLSPreview',
        success: function (result) {
            console.log('success!');
            html = result;
            return html;
        }
    });
    return html;
}

function CreateNewOLS(objectToCreate, servId, labId) {
    var success = 0;
    $.ajax({
        type: 'GET',
        async: false,
        contentType: 'html',
        data: {
            offeredService: JSON.stringify(objectToCreate), serviceId: servId, labourerId: labId
        },
        url: '/Admin/CreateOLS',
        success: function (result) {
            console.log('success!');
            success = result;
        }
    });
    return success;
}


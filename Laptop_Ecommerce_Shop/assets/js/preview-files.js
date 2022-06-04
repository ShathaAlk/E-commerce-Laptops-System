//preview image before uploading to the database
function imagePreview(evt) {
    var files = evt.target.files; // FileList object
    // Loop through the FileList and render image files as thumbnails.
    for (var i = 0, f; f = files[i]; i++) {
        // Only process image files.
        if (!f.type.match('image.*')) {
            document.getElementById("imgFiles").value = "";
            document.getElementById('imgList').innerHTML = "";
            divModal.style.display = 'block';
            continue;
        }
        document.getElementById('imgList').innerHTML = "";
        var reader = new FileReader();

        // Closure to capture the file information.
        reader.onload = (function (theFile) {
            return function (e) {
                // Render thumbnail.
                var span = document.createElement('span');
                span.innerHTML = ['<img class="img-thumb" src="', e.target.result,
                    '" title="', escape(theFile.name), '"/>'
                ].join('');
                document.getElementById('imgList').insertBefore(span, null);
            };
        })(f);
        // Read in the image file as a data URL.
        reader.readAsDataURL(f);
    }
}
//preview video before uploading to the database
function videoPreview(evt) {
    var files = evt.target.files; // FileList object
    // Loop through the FileList and render image files as thumbnails.
    for (var i = 0, f; f = files[i]; i++) {
        document.getElementById('videoList').style.display = "block";

        // Only process video files.
        if (!f.type.match('video.*')) {
            divModal.style.display = 'block';
            document.getElementById("videoFiles").value = "";
            document.getElementById('videoList').style.display = "none";
            continue;
        }
        document.getElementById('videoList').innerHTML = "";
        var reader = new FileReader();
        // Closure to capture the file information.
        reader.onload = (function (theFile) {
            return function (e) {
                // Render thumbnail.
                var span = document.createElement('span');
                span.innerHTML = ['<video class="video-thumb" src="', e.target.result,
                    '" title="', escape(theFile.name), '" controls autoplay/>'
                ].join('');
                document.getElementById('videoList').insertBefore(span, null);
            };
        })(f);
        // Read in the image file as a data URL.
        reader.readAsDataURL(f);
    }
}

//pop up message
// Get the modal
var divModal = document.getElementById("divModal");
// Get the <span> element that closes the modal
var span = document.getElementsByClassName("ok")[0];
// When the user clicks on <span> (ok), close the modal
span.onclick = function () {
    divModal.style.display = "none";
}
// When the user clicks anywhere outside of the modal, close it
window.onclick = function (event) {
    if (event.target == divModal) {
        divModal.style.display = "none";
    }
}
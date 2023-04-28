window.BlazorModalExtensions =
{
    Draggable: function () {
        let draggableElements = document.getElementsByClassName("blazored-modal-draggable");
        if (!draggableElements || draggableElements.length == 0) return;
        const modalWindow = draggableElements[0];
        modalWindow.style.width = document.getElementsByClassName("blazored-modal-content")[0].children[0].clientWidth + "px";
        const maxTop = screen.height - modalWindow.offsetHeight;
        const maxLeft = screen.width - modalWindow.offsetWidth;
        modalWindow.parentElement.style.width = "0%";
        modalWindow.parentElement.style.height = "0%";
        modalWindow.style.top = "50px";
        modalWindow.style.left = "0px";
        dragElement(modalWindow);

        function dragElement() {
            let diffPosX = 0, diffPosY = 0, startPosX = 0, startPosY = 0;
            modalWindow.onmousedown = dragMouseDown;

            function dragMouseDown(e) {
                e = e || window.event;

                startPosX = e.clientX;
                startPosY = e.clientY;
                document.onmouseup = closeDragElement;

                document.onmousemove = elementDrag;
            }

            function elementDrag(e) {
                e = e || window.event;

                diffPosX = startPosX - e.clientX;
                diffPosY = startPosY - e.clientY;
                startPosX = e.clientX;
                startPosY = e.clientY;

                modalWindow.style.top = Math.min(maxTop, Math.max(50, (modalWindow.offsetTop - diffPosY))) + "px";
                modalWindow.style.left = Math.min(maxLeft, Math.max(0, (modalWindow.offsetLeft - diffPosX))) + "px";
            }

            function closeDragElement() {
                document.onmouseup = null;
                document.onmousemove = null;
            }
        }
    }
}
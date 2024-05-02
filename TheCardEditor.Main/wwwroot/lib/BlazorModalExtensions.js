window.BlazorModalExtensions =
{
    Draggable: async function (ref, guid, method, initialTop, initialLeft) {
        let draggableElements = document.getElementsByClassName("blazored-modal-draggable");
        while (!draggableElements || draggableElements.length == 0) {
            await new Promise(resolve => setTimeout(resolve, 10));
        }
        const modalWindow = draggableElements[0];
        modalWindow.style.width = document.getElementsByClassName("bm-content")[0].children[0].clientWidth + "px";
        const maxTop = screen.height - modalWindow.offsetHeight;
        const maxLeft = screen.width - modalWindow.offsetWidth;
        modalWindow.parentElement.style.width = "0%";
        modalWindow.parentElement.style.height = "0%";
        modalWindow.style.top = `${initialTop ?? 50}px`;
        modalWindow.style.left = `${initialLeft ?? 0}px`;
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
                const newTop = Math.min(maxTop, Math.max(50, (modalWindow.offsetTop - diffPosY)));
                const newLeft = Math.min(maxLeft, Math.max(0, (modalWindow.offsetLeft - diffPosX)));
                modalWindow.style.top = newTop + "px";
                modalWindow.style.left = newLeft + "px";
                ref.invokeMethodAsync(method, guid, newTop, newLeft);
            }

            function closeDragElement() {
                document.onmouseup = null;
                document.onmousemove = null;
            }
        }
    }
}
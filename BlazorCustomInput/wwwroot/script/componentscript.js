export function setFocusElement(element) {
    if (element instanceof HTMLElement) {
        element.focus();
    }
};
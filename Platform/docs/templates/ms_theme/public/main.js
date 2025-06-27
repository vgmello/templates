function setupSidebar() {
    const affix = document.getElementById("affix");
    if (!affix) {
        return;
    }

    affix.addEventListener("click", (e) => {
        if (e.target.matches("a")) {
            const links = affix.querySelectorAll("a");
            links.forEach((l) => l.removeAttribute("data-active"));
            e.target.setAttribute("data-active", "true");
        }
    });
}

export default {
    start: () => {
        setupSidebar();
    },
};

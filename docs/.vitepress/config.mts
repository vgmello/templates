import { defineConfig, type MarkdownOptions } from "vitepress";

import MermaidExample from "./plugins/mermaid";
import SnippetPluginExt from "./plugins/snippet";
import TocSidebar from "./plugins/tocSidebar";
import AdrSidebar from "./plugins/adr/adrSidebar";
import EventsSidebar from "./plugins/eventsSidebar";

const REF_DIR = "reference/toc.yml";
const ADR_DIR = "arch/adr";

const allMarkdownTransformers: MarkdownOptions = {
    theme: {
        light: "github-light",
        dark: "github-dark",
    },

    preConfig: (md) => {
        SnippetPluginExt(md);
    },

    config: (md) => {
        MermaidExample(md);
    },
};

export default defineConfig({
    title: "Billing Solution",
    description: "Comprehensive billing management system with Cashiers, Invoices, and Bills",
    markdown: allMarkdownTransformers,
    themeConfig: {
        nav: [
            { text: "Home", link: "/" },
            { text: "API", link: "http://localhost:8101/scalar" },
            { text: "Events", link: "/events/" },
            { text: "Guide", link: "/guide/" },
            { text: "Architecture", link: "/arch/" },
            { text: "Reference", link: "/reference/Billing" },
        ],

        editLink: {
            pattern: 'https://github.com/user/project/edit/main/docs/:path'
        },

        search: {
            provider: "local",
        },

        sidebar: {
            "/arch/": {
                base: "/arch",
                items: [
                    {
                        text: "Architecture",
                        collapsed: false,
                        items: [
                            { text: "Overview", link: "/" },
                            { text: "Event-Driven Architecture", link: "/eda" },
                            { text: "Database Design", link: "/database" },
                            { text: "Background Processing", link: "/background-processing" },
                            { text: "ADRs", base: "/arch/adr", items: AdrSidebar(ADR_DIR) },
                        ],
                    },
                    {
                        text: "Patterns",
                        collapsed: false,
                        items: [
                            { text: "CQRS Implementation", link: "/cqrs" },
                            { text: "Error Handling", link: "/error-handling" },
                            { text: "Testing Strategies", link: "/testing" },
                        ],
                    },
                ],
            },
            "/guide/": {
                base: "/guide",
                items: [
                    {
                        text: "Introduction",
                        collapsed: false,
                        items: [
                            { text: "Billing Solution", link: "/" },
                            { text: "Getting Started", link: "/getting-started" },
                        ],
                    },
                    {
                        text: "Cashiers",
                        collapsed: false,
                        items: [{ text: "Cashiers Overview", link: "/cashiers/" }],
                    },
                    {
                        text: "Invoices",
                        collapsed: false,
                        items: [{ text: "Invoices Overview", link: "/invoices/" }],
                    },
                    {
                        text: "Bills",
                        collapsed: false,
                        items: [{ text: "Bills Overview", link: "/bills/" }],
                    },
                    {
                        text: "Developer Guide",
                        collapsed: false,
                        items: [
                            { text: "Documentation Layout", link: "/documentation-layout" },
                            { text: "Running Database Queries and Commands", link: "/dbcommand-usage-guide.md" },
                            { text: "Debugging Tips", link: "debugging" },
                        ],
                    },
                ],
            },
            "/events/": {
                base: "/events",
                items: EventsSidebar(),
            },
            "/reference": {
                base: "/reference",
                items: TocSidebar(REF_DIR),
            },
        },

        socialLinks: [{ icon: "github", link: "https://github.com/" }],

        footer: {
            copyright: "Copyright © 2025 Billing Team",
        },
    },
    lastUpdated: true,
    cleanUrls: true,
});

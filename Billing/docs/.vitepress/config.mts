import { defineConfig } from 'vitepress'

// https://vitepress.dev/reference/site-config
export default defineConfig({
  title: "Billing Service",
  description: "Comprehensive billing service documentation with API reference, architecture, and development guides",
  base: '/',
  srcDir: '.',
  outDir: '.vitepress/dist',
  
  themeConfig: {
    // https://vitepress.dev/reference/default-theme-config
    nav: [
      { text: 'Home', link: '/' },
      { text: 'Architecture', link: '/architecture' },
      { text: 'API Reference', link: '/api-reference' },
      { text: 'Database', link: '/database' },
      { text: 'Frontend', link: '/frontend' }
    ],

    sidebar: [
      {
        text: 'Documentation',
        items: [
          { text: 'Overview', link: '/' },
          { text: 'Architecture Overview', link: '/architecture' },
          { text: 'API Reference', link: '/api-reference' },
          { text: 'Database Schema', link: '/database' },
          { text: 'Frontend Development', link: '/frontend' }
        ]
      }
    ],

    socialLinks: [
      { icon: 'github', link: 'https://github.com/your-org/billing-service' }
    ],

    search: {
      provider: 'local'
    },

    footer: {
      message: 'Billing Service Documentation',
      copyright: 'Copyright © 2025 Billing Service'
    }
  },

  markdown: {
    lineNumbers: true,
    anchor: {
      permalink: true
    }
  },

  ignoreDeadLinks: [
    // Allow localhost URLs for development
    /^http:\/\/localhost:\d+/
  ]
})

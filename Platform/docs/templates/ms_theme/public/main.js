function setupSidebar() {
  const affix = document.getElementById('affix');
  if (!affix) {
    return;
  }

  affix.addEventListener('click', (e) => {
    if (e.target.matches('a')) {
      const links = affix.querySelectorAll('a');
      links.forEach(l => l.removeAttribute('data-active'));
      e.target.setAttribute('data-active', 'true');
    }
  });
}

/**
 * Initialize breadcrumb navigation
 */
function initializeBreadcrumbs() {
    const contentDiv = document.querySelector('article[role="main"]');
    if (!contentDiv) return;

    const breadcrumbContainer = createBreadcrumbContainer();
    if (breadcrumbContainer) {
        contentDiv.parentNode.insertBefore(breadcrumbContainer, contentDiv);
    }
}

/**
 * Create breadcrumb container based on current page location
 */
function createBreadcrumbContainer() {
    const currentPath = window.location.pathname;
    const breadcrumbs = generateBreadcrumbs(currentPath);
    
    if (breadcrumbs.length === 0) return null;

    const container = document.createElement('div');
    container.className = 'breadcrumb-container';
    
    const nav = document.createElement('nav');
    nav.setAttribute('aria-label', 'breadcrumb');
    
    const ol = document.createElement('ol');
    ol.className = 'breadcrumb';
    
    // Add home link
    const homeLi = document.createElement('li');
    homeLi.className = 'breadcrumb-item';
    
    const homeLink = document.createElement('a');
    homeLink.href = getRelativeRoot() + 'index.html';
    homeLink.title = 'Home';
    homeLink.innerHTML = '<i class="bi bi-house-door"></i><span class="sr-only">Home</span>';
    
    homeLi.appendChild(homeLink);
    ol.appendChild(homeLi);
    
    // Add breadcrumb items
    breadcrumbs.forEach((breadcrumb, index) => {
        const li = document.createElement('li');
        li.className = 'breadcrumb-item';
        
        if (index === breadcrumbs.length - 1) {
            // Last item (current page) - no link
            li.className += ' active';
            li.setAttribute('aria-current', 'page');
            li.textContent = breadcrumb.name;
        } else {
            // Intermediate items - with links
            const link = document.createElement('a');
            link.href = getRelativeRoot() + breadcrumb.href;
            link.title = breadcrumb.name;
            link.textContent = breadcrumb.name;
            li.appendChild(link);
        }
        
        ol.appendChild(li);
    });
    
    nav.appendChild(ol);
    container.appendChild(nav);
    
    return container;
}

/**
 * Generate breadcrumb items based on URL path
 */
function generateBreadcrumbs(path) {
    const breadcrumbs = [];
    
    // Remove leading/trailing slashes and split path
    const pathParts = path.replace(/^\/+|\/+$/g, '').split('/').filter(part => part);
    
    // Skip if we're on the home page
    if (pathParts.length === 0 || pathParts[pathParts.length - 1] === 'index.html') {
        return breadcrumbs;
    }
    
    // Map common paths to readable names
    const pathNameMap = {
        'content': 'Documentation',
        'api': 'API Reference',
        'architecture': 'Architecture',
        'database-integration': 'Database Integration',
        'messaging': 'Messaging',
        'healthchecks': 'Health Checks',
        'logging': 'Logging',
        'opentelemetry': 'OpenTelemetry',
        'source-generators': 'Source Generators',
        'extensions': 'Extensions',
        'openapi': 'OpenAPI',
        'overview.html': 'Overview',
        'setup.html': 'Setup',
        'service-defaults.html': 'Service Defaults',
        'grpc-integration.html': 'gRPC Integration',
        'endpoint-filters.html': 'Endpoint Filters',
        'xml-documentation.html': 'XML Documentation',
        'transformers.html': 'Transformers',
        'dapper-extensions.html': 'Dapper Extensions',
        'source-generators.html': 'Source Generators',
        'wolverine-integration.html': 'Wolverine Integration',
        'cloudevents.html': 'CloudEvents',
        'kafka.html': 'Kafka',
        'middlewares.html': 'Middlewares',
        'telemetry.html': 'Telemetry',
        'dynamic-log-levels.html': 'Dynamic Log Levels',
        'dbcommand-generator.html': 'DbCommand Generator',
        'string-extensions.html': 'String Extensions',
        'result-pattern.html': 'Result Pattern'
    };
    
    let currentPath = '';
    
    for (let i = 0; i < pathParts.length; i++) {
        const part = pathParts[i];
        currentPath += (i === 0 ? '' : '/') + part;
        
        const isLastPart = i === pathParts.length - 1;
        const displayName = pathNameMap[part] || formatPathName(part);
        
        breadcrumbs.push({
            name: displayName,
            href: isLastPart ? null : currentPath + '/'
        });
    }
    
    return breadcrumbs;
}

/**
 * Format path name for display
 */
function formatPathName(pathName) {
    return pathName
        .replace(/-/g, ' ')
        .replace(/\.html$/, '')
        .replace(/\b\w/g, l => l.toUpperCase());
}

/**
 * Get relative root path
 */
function getRelativeRoot() {
    const currentPath = window.location.pathname;
    const depth = (currentPath.match(/\//g) || []).length - 1;
    
    if (depth <= 1) return './';
    
    return '../'.repeat(depth - 1);
}

/**
 * Initialize code copy buttons
 */
function initializeCodeCopyButtons() {
    const codeBlocks = document.querySelectorAll('pre code');
    
    codeBlocks.forEach(codeBlock => {
        const pre = codeBlock.parentElement;
        if (!pre || pre.querySelector('.copy-button')) return;
        
        const copyButton = document.createElement('button');
        copyButton.className = 'copy-button btn btn-sm btn-outline-secondary';
        copyButton.innerHTML = '<i class="bi bi-clipboard"></i>';
        copyButton.title = 'Copy to clipboard';
        copyButton.style.position = 'absolute';
        copyButton.style.top = '0.5rem';
        copyButton.style.right = '0.5rem';
        copyButton.style.zIndex = '10';
        
        pre.style.position = 'relative';
        
        copyButton.addEventListener('click', () => {
            navigator.clipboard.writeText(codeBlock.textContent).then(() => {
                copyButton.innerHTML = '<i class="bi bi-check"></i>';
                copyButton.title = 'Copied!';
                
                setTimeout(() => {
                    copyButton.innerHTML = '<i class="bi bi-clipboard"></i>';
                    copyButton.title = 'Copy to clipboard';
                }, 2000);
            });
        });
        
        pre.appendChild(copyButton);
    });
}

/**
 * Initialize table of contents highlighting
 */
function initializeTableOfContentsHighlighting() {
    const tocLinks = document.querySelectorAll('#affix a[href^="#"]');
    const headings = document.querySelectorAll('h1[id], h2[id], h3[id], h4[id], h5[id], h6[id]');
    
    if (tocLinks.length === 0 || headings.length === 0) return;
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            const link = document.querySelector(`#affix a[href="#${entry.target.id}"]`);
            if (link) {
                if (entry.isIntersecting) {
                    // Remove active class from all links
                    tocLinks.forEach(l => l.removeAttribute('data-active'));
                    // Add active class to current link
                    link.setAttribute('data-active', 'true');
                }
            }
        });
    }, {
        rootMargin: '-20% 0% -35% 0%'
    });
    
    headings.forEach(heading => observer.observe(heading));
}

export default {
  start: () => {
      setupSidebar();
      initializeBreadcrumbs();
      initializeCodeCopyButtons();
      initializeTableOfContentsHighlighting();
  },
};

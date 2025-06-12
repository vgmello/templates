<script lang="ts">
  import { page } from '$app/stores';

  let isMenuOpen = $state(false);

  const navigation = [
    { name: 'Dashboard', href: '/', icon: '🏠' },
    { name: 'Ledgers', href: '/ledgers', icon: '📊' },
    { name: 'Cashiers', href: '/cashiers', icon: '👥' }
  ];

  function isActive(href: string): boolean {
    if (href === '/') {
      return $page.url.pathname === '/';
    }
    return $page.url.pathname.startsWith(href);
  }
</script>

<nav class="navigation">
  <div class="nav-container">
    <div class="nav-brand">
      <a href="/" class="brand-link">
        <span class="brand-icon">⚡</span>
        <span class="brand-text">Operations</span>
      </a>
    </div>

    <!-- Desktop Navigation -->
    <div class="nav-menu desktop-menu">
      {#each navigation as item}
        <a 
          href={item.href} 
          class="nav-link {isActive(item.href) ? 'active' : ''}"
        >
          <span class="nav-icon">{item.icon}</span>
          <span class="nav-text">{item.name}</span>
        </a>
      {/each}
    </div>

    <!-- Mobile Menu Button -->
    <button 
      class="mobile-menu-button"
      onclick={() => isMenuOpen = !isMenuOpen}
      aria-label="Toggle menu"
    >
      <span class="hamburger"></span>
      <span class="hamburger"></span>
      <span class="hamburger"></span>
    </button>
  </div>

  <!-- Mobile Navigation -->
  {#if isMenuOpen}
    <div class="mobile-menu">
      {#each navigation as item}
        <a 
          href={item.href} 
          class="mobile-nav-link {isActive(item.href) ? 'active' : ''}"
          onclick={() => isMenuOpen = false}
        >
          <span class="nav-icon">{item.icon}</span>
          <span class="nav-text">{item.name}</span>
        </a>
      {/each}
    </div>
  {/if}
</nav>

<style>
  .navigation {
    background: white;
    border-bottom: 1px solid #e5e7eb;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    position: sticky;
    top: 0;
    z-index: 50;
  }

  .nav-container {
    max-width: 1400px;
    margin: 0 auto;
    padding: 0 20px;
    display: flex;
    justify-content: space-between;
    align-items: center;
    height: 64px;
  }

  .nav-brand {
    display: flex;
    align-items: center;
  }

  .brand-link {
    display: flex;
    align-items: center;
    text-decoration: none;
    color: #1f2937;
    font-weight: 700;
    font-size: 1.25rem;
  }

  .brand-icon {
    font-size: 1.5rem;
    margin-right: 8px;
  }

  .brand-text {
    color: #1f2937;
  }

  .desktop-menu {
    display: flex;
    align-items: center;
    gap: 8px;
  }

  .nav-link {
    display: flex;
    align-items: center;
    padding: 8px 16px;
    border-radius: 6px;
    text-decoration: none;
    color: #6b7280;
    font-weight: 500;
    transition: all 0.2s;
  }

  .nav-link:hover {
    background: #f3f4f6;
    color: #374151;
  }

  .nav-link.active {
    background: #dbeafe;
    color: #1e40af;
  }

  .nav-icon {
    margin-right: 8px;
    font-size: 1rem;
  }

  .nav-text {
    font-size: 0.875rem;
  }

  .mobile-menu-button {
    display: none;
    flex-direction: column;
    background: none;
    border: none;
    cursor: pointer;
    padding: 8px;
  }

  .hamburger {
    width: 20px;
    height: 2px;
    background: #374151;
    margin: 2px 0;
    transition: 0.3s;
  }

  .mobile-menu {
    display: none;
    background: white;
    border-top: 1px solid #e5e7eb;
    padding: 16px 20px;
  }

  .mobile-nav-link {
    display: flex;
    align-items: center;
    padding: 12px 0;
    text-decoration: none;
    color: #6b7280;
    font-weight: 500;
    border-bottom: 1px solid #f3f4f6;
  }

  .mobile-nav-link:last-child {
    border-bottom: none;
  }

  .mobile-nav-link:hover,
  .mobile-nav-link.active {
    color: #1e40af;
  }

  @media (max-width: 768px) {
    .desktop-menu {
      display: none;
    }

    .mobile-menu-button {
      display: flex;
    }

    .mobile-menu {
      display: block;
    }
  }
</style>
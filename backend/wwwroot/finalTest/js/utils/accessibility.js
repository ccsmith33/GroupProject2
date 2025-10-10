// Accessibility Utilities

/**
 * Add ARIA labels to interactive elements
 * @param {HTMLElement} element - The element to add ARIA labels to
 * @param {Object} options - ARIA label options
 */
export function addAriaLabels(element, options = {}) {
  if (!element) return;

  const {
    label,
    describedBy,
    expanded,
    controls,
    owns,
    live,
    atomic,
    relevant,
    hidden,
    role,
    tabindex,
  } = options;

  if (label) element.setAttribute("aria-label", label);
  if (describedBy) element.setAttribute("aria-describedby", describedBy);
  if (expanded !== undefined) element.setAttribute("aria-expanded", expanded);
  if (controls) element.setAttribute("aria-controls", controls);
  if (owns) element.setAttribute("aria-owns", owns);
  if (live) element.setAttribute("aria-live", live);
  if (atomic !== undefined) element.setAttribute("aria-atomic", atomic);
  if (relevant) element.setAttribute("aria-relevant", relevant);
  if (hidden !== undefined) element.setAttribute("aria-hidden", hidden);
  if (role) element.setAttribute("role", role);
  if (tabindex !== undefined) element.setAttribute("tabindex", tabindex);
}

/**
 * Create a focus trap for modals and dialogs
 * @param {HTMLElement} container - The container to trap focus within
 * @param {HTMLElement} firstFocus - The first element to focus
 * @param {HTMLElement} lastFocus - The last element to focus
 */
export function createFocusTrap(
  container,
  firstFocus = null,
  lastFocus = null
) {
  if (!container) return null;

  const focusableElements = container.querySelectorAll(
    'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
  );

  const first = firstFocus || focusableElements[0];
  const last = lastFocus || focusableElements[focusableElements.length - 1];

  const handleKeyDown = (e) => {
    if (e.key === "Tab") {
      if (e.shiftKey) {
        if (document.activeElement === first) {
          e.preventDefault();
          last.focus();
        }
      } else {
        if (document.activeElement === last) {
          e.preventDefault();
          first.focus();
        }
      }
    }
  };

  container.addEventListener("keydown", handleKeyDown);

  // Focus the first element
  if (first) first.focus();

  return {
    destroy: () => {
      container.removeEventListener("keydown", handleKeyDown);
    },
  };
}

/**
 * Announce text to screen readers
 * @param {string} text - Text to announce
 * @param {string} priority - Priority level (polite, assertive, off)
 */
export function announceToScreenReader(text, priority = "polite") {
  const announcement = document.createElement("div");
  announcement.setAttribute("aria-live", priority);
  announcement.setAttribute("aria-atomic", "true");
  announcement.className = "sr-only";
  announcement.textContent = text;

  document.body.appendChild(announcement);

  // Remove after announcement
  setTimeout(() => {
    document.body.removeChild(announcement);
  }, 1000);
}

/**
 * Create skip navigation links
 * @param {Array} links - Array of skip link objects
 */
export function createSkipLinks(links = []) {
  const skipContainer = document.createElement("div");
  skipContainer.className = "skip-links";
  skipContainer.setAttribute("aria-label", "Skip navigation");

  const defaultLinks = [
    { href: "#main-content", text: "Skip to main content" },
    { href: "#navigation", text: "Skip to navigation" },
    { href: "#search", text: "Skip to search" },
  ];

  const allLinks = [...defaultLinks, ...links];

  allLinks.forEach((link) => {
    const skipLink = document.createElement("a");
    skipLink.href = link.href;
    skipLink.textContent = link.text;
    skipLink.className = "skip-link";
    skipLink.addEventListener("click", (e) => {
      e.preventDefault();
      const target = document.querySelector(link.href);
      if (target) {
        target.focus();
        target.scrollIntoView();
      }
    });
    skipContainer.appendChild(skipLink);
  });

  return skipContainer;
}

/**
 * Add keyboard navigation to a list of elements
 * @param {NodeList|Array} elements - Elements to add keyboard navigation to
 * @param {Object} options - Navigation options
 */
export function addKeyboardNavigation(elements, options = {}) {
  const {
    orientation = "vertical", // vertical, horizontal, both
    loop = true,
    onSelect = null,
    onActivate = null,
  } = options;

  if (!elements || elements.length === 0) return;

  const elementArray = Array.from(elements);
  let currentIndex = 0;

  const handleKeyDown = (e) => {
    const { key } = e;
    let newIndex = currentIndex;

    switch (key) {
      case "ArrowUp":
        if (orientation === "vertical" || orientation === "both") {
          e.preventDefault();
          newIndex =
            currentIndex > 0
              ? currentIndex - 1
              : loop
              ? elementArray.length - 1
              : currentIndex;
        }
        break;
      case "ArrowDown":
        if (orientation === "vertical" || orientation === "both") {
          e.preventDefault();
          newIndex =
            currentIndex < elementArray.length - 1
              ? currentIndex + 1
              : loop
              ? 0
              : currentIndex;
        }
        break;
      case "ArrowLeft":
        if (orientation === "horizontal" || orientation === "both") {
          e.preventDefault();
          newIndex =
            currentIndex > 0
              ? currentIndex - 1
              : loop
              ? elementArray.length - 1
              : currentIndex;
        }
        break;
      case "ArrowRight":
        if (orientation === "horizontal" || orientation === "both") {
          e.preventDefault();
          newIndex =
            currentIndex < elementArray.length - 1
              ? currentIndex + 1
              : loop
              ? 0
              : currentIndex;
        }
        break;
      case "Home":
        e.preventDefault();
        newIndex = 0;
        break;
      case "End":
        e.preventDefault();
        newIndex = elementArray.length - 1;
        break;
      case "Enter":
      case " ":
        e.preventDefault();
        if (onActivate) onActivate(elementArray[currentIndex], currentIndex);
        break;
      case "Escape":
        e.preventDefault();
        if (onSelect) onSelect(null);
        break;
    }

    if (newIndex !== currentIndex) {
      // Remove focus from current element
      elementArray[currentIndex].setAttribute("tabindex", "-1");

      // Update current index
      currentIndex = newIndex;

      // Focus new element
      elementArray[currentIndex].setAttribute("tabindex", "0");
      elementArray[currentIndex].focus();

      if (onSelect) onSelect(elementArray[currentIndex], currentIndex);
    }
  };

  // Add event listeners to all elements
  elementArray.forEach((element, index) => {
    element.setAttribute("tabindex", index === 0 ? "0" : "-1");
    element.setAttribute("role", "option");
    element.addEventListener("keydown", handleKeyDown);
  });

  return {
    destroy: () => {
      elementArray.forEach((element) => {
        element.removeEventListener("keydown", handleKeyDown);
        element.removeAttribute("tabindex");
        element.removeAttribute("role");
      });
    },
    focus: (index) => {
      if (index >= 0 && index < elementArray.length) {
        elementArray[currentIndex].setAttribute("tabindex", "-1");
        currentIndex = index;
        elementArray[currentIndex].setAttribute("tabindex", "0");
        elementArray[currentIndex].focus();
      }
    },
  };
}

/**
 * Add ARIA attributes to form elements
 * @param {HTMLElement} form - The form element
 */
export function enhanceFormAccessibility(form) {
  if (!form) return;

  const inputs = form.querySelectorAll("input, select, textarea");

  inputs.forEach((input) => {
    const label = form.querySelector(`label[for="${input.id}"]`);
    const helpText = form.querySelector(`#${input.id}-help`);
    const errorElement = form.querySelector(`#${input.id}-error`);

    // Add ARIA attributes
    if (label) {
      input.setAttribute("aria-labelledby", label.id || `label-${input.id}`);
      if (!label.id) label.id = `label-${input.id}`;
    }

    if (helpText) {
      input.setAttribute("aria-describedby", helpText.id || `help-${input.id}`);
      if (!helpText.id) helpText.id = `help-${input.id}`;
    }

    if (errorElement) {
      input.setAttribute(
        "aria-describedby",
        [
          input.getAttribute("aria-describedby"),
          errorElement.id || `error-${input.id}`,
        ]
          .filter(Boolean)
          .join(" ")
      );
      if (!errorElement.id) errorElement.id = `error-${input.id}`;
    }

    // Add required attribute
    if (input.hasAttribute("required")) {
      input.setAttribute("aria-required", "true");
    }

    // Add validation attributes
    if (input.type === "email") {
      input.setAttribute("aria-invalid", "false");
    }
  });
}

/**
 * Create a high contrast mode toggle
 */
export function createHighContrastToggle() {
  const toggle = document.createElement("button");
  toggle.className = "btn btn-outline-secondary";
  toggle.innerHTML = '<i class="bi bi-contrast"></i> High Contrast';
  toggle.setAttribute("aria-label", "Toggle high contrast mode");

  let isHighContrast = false;

  toggle.addEventListener("click", () => {
    isHighContrast = !isHighContrast;
    document.body.classList.toggle("high-contrast", isHighContrast);
    toggle.setAttribute("aria-pressed", isHighContrast);
    announceToScreenReader(
      isHighContrast
        ? "High contrast mode enabled"
        : "High contrast mode disabled"
    );
  });

  return toggle;
}

/**
 * Add focus indicators to interactive elements
 */
export function addFocusIndicators() {
  const style = document.createElement("style");
  style.textContent = `
    .focus-visible {
      outline: 2px solid #007bff;
      outline-offset: 2px;
    }
    
    .high-contrast .focus-visible {
      outline: 3px solid #ffffff;
      outline-offset: 3px;
    }
    
    .skip-links {
      position: absolute;
      top: -40px;
      left: 6px;
      z-index: 1000;
    }
    
    .skip-link {
      position: absolute;
      top: -40px;
      left: 6px;
      background: #000;
      color: #fff;
      padding: 8px 16px;
      text-decoration: none;
      border-radius: 4px;
      z-index: 1000;
    }
    
    .skip-link:focus {
      top: 6px;
    }
    
    .sr-only {
      position: absolute;
      width: 1px;
      height: 1px;
      padding: 0;
      margin: -1px;
      overflow: hidden;
      clip: rect(0, 0, 0, 0);
      white-space: nowrap;
      border: 0;
    }
  `;

  document.head.appendChild(style);
}

/**
 * Initialize accessibility features
 */
export function initializeAccessibility() {
  // Add focus indicators
  addFocusIndicators();

  // Add skip links
  const skipLinks = createSkipLinks();
  document.body.insertBefore(skipLinks, document.body.firstChild);

  // Add high contrast toggle
  const contrastToggle = createHighContrastToggle();
  const header = document.querySelector(".navbar");
  if (header) {
    header.appendChild(contrastToggle);
  }

  // Add focus management for modals
  document.addEventListener("keydown", (e) => {
    if (e.key === "Escape") {
      const modal = document.querySelector(".modal.show");
      if (modal) {
        const closeButton = modal.querySelector("[data-bs-dismiss='modal']");
        if (closeButton) closeButton.click();
      }
    }
  });

  // Announce page changes
  const observer = new MutationObserver((mutations) => {
    mutations.forEach((mutation) => {
      if (mutation.type === "childList") {
        const addedNodes = Array.from(mutation.addedNodes);
        addedNodes.forEach((node) => {
          if (node.nodeType === Node.ELEMENT_NODE) {
            const heading = node.querySelector("h1, h2, h3");
            if (heading) {
              announceToScreenReader(`Page updated: ${heading.textContent}`);
            }
          }
        });
      }
    });
  });

  observer.observe(document.body, {
    childList: true,
    subtree: true,
  });
}

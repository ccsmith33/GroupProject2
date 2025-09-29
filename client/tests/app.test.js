// Frontend Unit Tests for app.js
// Run these tests by opening client/tests/test-runner.html in a browser

// Mock fetch for testing
const originalFetch = window.fetch;
window.fetch = jest.fn ? jest.fn() : function(url, options) {
    return Promise.resolve({
        ok: true,
        json: () => Promise.resolve([
            { id: 1, name: "Test Project", description: "Test Description", createdAt: "2025-01-01T00:00:00Z" }
        ]),
        status: 200
    });
};

// Test App class functionality
test('App constructor initializes correctly', () => {
    const app = new App();
    expect(app.apiBaseUrl).toBe("https://localhost:7000/api");
    expect(app.currentPage).toBe("home");
});

test('escapeHtml escapes HTML characters', () => {
    const app = new App();
    const result = app.escapeHtml("<script>alert('xss')</script>");
    expect(result).toBe("&lt;script&gt;alert('xss')&lt;/script&gt;");
});

test('escapeHtml handles normal text', () => {
    const app = new App();
    const result = app.escapeHtml("Normal text");
    expect(result).toBe("Normal text");
});

// Test utility functions
test('utils.formatDate formats date correctly', () => {
    const dateString = "2025-01-01T00:00:00Z";
    const result = utils.formatDate(dateString);
    expect(result).toContain("January");
    expect(result).toContain("2025");
});

test('utils.formatDateTime formats datetime correctly', () => {
    const dateString = "2025-01-01T12:30:00Z";
    const result = utils.formatDateTime(dateString);
    expect(result).toContain("Jan");
    expect(result).toContain("2025");
    expect(result).toContain("12:30");
});

// Test API integration (mocked)
test('loadProjects calls API and handles response', async () => {
    const app = new App();
    
    // Mock DOM elements
    document.body.innerHTML = '<div id="projects-list"></div>';
    
    await app.loadProjects();
    
    const projectsList = document.getElementById('projects-list');
    expect(projectsList.innerHTML).toContain("Test Project");
});

// Test error handling
test('loadProjects handles API errors', async () => {
    const app = new App();
    
    // Mock fetch to return error
    window.fetch = function() {
        return Promise.reject(new Error('Network error'));
    };
    
    // Mock DOM elements
    document.body.innerHTML = '<div id="projects-list"></div>';
    
    await app.loadProjects();
    
    const projectsList = document.getElementById('projects-list');
    expect(projectsList.innerHTML).toContain("Error Loading Projects");
});

// Restore original fetch
window.fetch = originalFetch;

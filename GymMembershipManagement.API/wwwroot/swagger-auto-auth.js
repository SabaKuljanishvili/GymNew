/**
 * Swagger UI Auto-Authorization Script
 * 
 * This script automatically handles JWT token management for Swagger UI:
 * 1. Intercepts login responses and saves token to localStorage
 * 2. Loads token from localStorage on page load
 * 3. Automatically authorizes Swagger UI with saved token
 * 4. Adds token to all API requests
 */

(function() {
    console.log('🔐 Swagger Auto-Auth Script Initialized');

    // Configuration
    const TOKEN_KEY = 'gym_auth_token';
    const API_LOGIN_ENDPOINT = '/api/user/Login';

    /**
     * Save token to localStorage
     */
    function saveToken(token) {
        if (token) {
            localStorage.setItem(TOKEN_KEY, token);
            console.log('✅ Token saved to localStorage');
        }
    }

    /**
     * Get token from localStorage
     */
    function getToken() {
        return localStorage.getItem(TOKEN_KEY);
    }

    /**
     * Remove token from localStorage
     */
    function removeToken() {
        localStorage.removeItem(TOKEN_KEY);
        console.log('🗑️ Token removed from localStorage');
    }

    /**
     * Authorize Swagger UI with token
     */
    function authorizeSwaggerUI(token) {
        // Wait for Swagger UI to initialize
        if (window.ui && typeof window.ui.preauthorizeApiKey === 'function') {
            window.ui.preauthorizeApiKey('Bearer', token);
            console.log('✅ Swagger UI authorized with token');
            return true;
        }
        return false;
    }

    /**
     * Intercept all fetch requests to add authorization header
     */
    function setupFetchInterceptor() {
        const originalFetch = window.fetch;

        window.fetch = function(...args) {
            const [resource, config] = args;
            const token = getToken();

            // Add authorization header to all requests if token exists
            if (token && typeof resource === 'string') {
                const headers = (config && config.headers) || {};
                
                // Only add if not already present
                if (!headers['Authorization']) {
                    headers['Authorization'] = `Bearer ${token}`;
                }

                // Update config
                if (!config) {
                    args[1] = { headers };
                } else {
                    config.headers = headers;
                }
            }

            // Call original fetch
            return originalFetch.apply(this, args)
                .then(async (response) => {
                    // Clone response to avoid consuming it
                    const clonedResponse = response.clone();

                    // Check if this is a login request
                    if (resource.includes(API_LOGIN_ENDPOINT) && response.ok) {
                        try {
                            const data = await clonedResponse.json();

                            // If response contains token
                            if (data.token) {
                                console.log('🔓 Login successful');
                                
                                // Save token
                                saveToken(data.token);

                                // Try to authorize Swagger UI
                                setTimeout(() => {
                                    if (authorizeSwaggerUI(data.token)) {
                                        console.log('🎉 Token auto-authorized in Swagger UI');
                                    }
                                }, 100);
                            }
                        } catch (error) {
                            console.log('ℹ️ Could not parse login response');
                        }
                    }

                    // Return original response
                    return response;
                });
        };
    }

    /**
     * Initialize auto-authorization on page load
     */
    function initializeOnPageLoad() {
        // Wait for page to load
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', initializeAutoAuth);
        } else {
            initializeAutoAuth();
        }
    }

    /**
     * Main initialization function
     */
    function initializeAutoAuth() {
        console.log('📍 Initializing auto-authorization');

        const token = getToken();

        if (token) {
            console.log('🔑 Found token in localStorage');
            
            // Try to authorize immediately
            if (authorizeSwaggerUI(token)) {
                console.log('✅ Pre-authorized with existing token');
            } else {
                // If Swagger UI not ready, retry after delay
                setTimeout(() => {
                    if (authorizeSwaggerUI(token)) {
                        console.log('✅ Pre-authorized with existing token (delayed)');
                    }
                }, 1000);
            }
        } else {
            console.log('ℹ️ No token found. Please login first.');
        }
    }

    /**
     * Add logout functionality
     */
    window.logoutFromSwagger = function() {
        removeToken();
        console.log('🚪 Logged out and token removed');
        // Optionally reload page
        location.reload();
    };

    /**
     * Add manual authorize function
     */
    window.authorizeSwaggerManually = function(token) {
        saveToken(token);
        if (authorizeSwaggerUI(token)) {
            console.log('✅ Manually authorized with token');
        }
    };

    // Setup fetch interceptor
    setupFetchInterceptor();

    // Initialize on page load
    initializeOnPageLoad();

    console.log('🔐 Swagger Auto-Auth Script Ready');
})();

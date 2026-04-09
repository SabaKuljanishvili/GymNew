// Auto-Authorization for Swagger UI
// Intercepts Login responses and automatically authorizes token

console.log('🔐 Swagger Auto-Auth script loaded');

// Store original fetch
const originalFetch = window.fetch;

// Override fetch to intercept API calls
window.fetch = function(...args) {
    const request = args[0];
    const options = args[1] || {};

    return originalFetch.apply(this, args)
        .then(async (response) => {
            // Clone response for reading
            const clonedResponse = response.clone();

            // Check if this is login endpoint and successful
            if (request.includes('/api/user/Login') && response.ok) {
                try {
                    const data = await clonedResponse.json();

                    // If response contains token
                    if (data.token) {
                        console.log('✅ Login successful, saving token to localStorage');

                        // Save token to localStorage
                        localStorage.setItem('swagger_api_key', data.token);

                        // Also save in sessionStorage for immediate use
                        sessionStorage.setItem('swagger_api_key', data.token);

                        // Notify Swagger UI to authorize (if available)
                        setTimeout(() => {
                            // Try to get Swagger UI instance
                            if (window.ui && window.ui.preauthorizeApiKey) {
                                window.ui.preauthorizeApiKey('Bearer', data.token);
                                console.log('✅ Token auto-authorized in Swagger UI');
                            }
                        }, 100);
                    }
                } catch (error) {
                    console.log('ℹ️ Note: Could not process login response');
                }
            }

            // Return original response
            return response;
        });
};

// When page loads, check if token exists and pre-authorize
window.addEventListener('load', function() {
    setTimeout(function() {
        const token = localStorage.getItem('swagger_api_key') || sessionStorage.getItem('swagger_api_key');

        if (token) {
            console.log('🔐 Found existing token, pre-authorizing...');

            // Try to authorize in Swagger UI if available
            if (window.ui && window.ui.preauthorizeApiKey) {
                window.ui.preauthorizeApiKey('Bearer', token);
                console.log('✅ Token pre-authorized');
            }
        }
    }, 500);
});

console.log('🔐 Swagger Auto-Auth initialized');

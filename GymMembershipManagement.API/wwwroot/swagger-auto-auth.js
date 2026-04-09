/**
 * Swagger UI Auto-Authorization Script
 *
 * Automatically manages JWT token:
 * 1. Intercepts login responses (fetch + XHR)
 * 2. Extracts token from response.token field
 * 3. Saves token to localStorage
 * 4. Calls Swagger UI's preauthorizeHttpBearer (correct method for Bearer/http scheme)
 * 5. Adds token to all subsequent API requests
 */

(function () {
    console.log('🔐 Swagger Auto-Auth Script Loaded');

    const TOKEN_KEY = 'gym_auth_token';
    const API_LOGIN_ENDPOINT = '/api/user/Login';

    // ─── localStorage helpers ──────────────────────────────────────────────────

    function saveToken(token) {
        if (!token) return false;
        localStorage.setItem(TOKEN_KEY, token);
        console.log('✅ Token saved to localStorage [' + TOKEN_KEY + ']');
        console.log('   Preview: ' + token.substring(0, 40) + '...');
        return true;
    }

    function getToken() {
        return localStorage.getItem(TOKEN_KEY) || null;
    }

    function removeToken() {
        localStorage.removeItem(TOKEN_KEY);
        console.log('🗑️ Token removed from localStorage');
    }

    // ─── Extract token from JSON response ─────────────────────────────────────

    function extractToken(data) {
        if (!data) return null;

        if (typeof data === 'string') {
            try { data = JSON.parse(data); } catch (e) { return null; }
        }

        // LoginResponseDTO returns { token: "..." }
        if (data.token && typeof data.token === 'string') {
            return data.token;
        }

        // Fallback: search nested objects one level deep
        for (const key of Object.keys(data)) {
            if (typeof data[key] === 'object' && data[key] !== null) {
                if (data[key].token && typeof data[key].token === 'string') {
                    return data[key].token;
                }
            }
        }

        console.warn('⚠️ Token field not found in response. Keys:', Object.keys(data));
        return null;
    }

    // ─── Authorize in Swagger UI ───────────────────────────────────────────────

    function authorizeInSwaggerUI(token) {
        if (!token) return false;

        function tryAuthorize(ui) {
            if (!ui) return false;

            // Method A: preauthorizeHttpBearer (correct for type:http / scheme:bearer)
            if (typeof ui.preauthorizeHttpBearer === 'function') {
                ui.preauthorizeHttpBearer('Bearer', token);
                console.log('✅ Authorized via preauthorizeHttpBearer()');
                return true;
            }

            // Method B: preauthorizeApiKey fallback
            if (typeof ui.preauthorizeApiKey === 'function') {
                ui.preauthorizeApiKey('Bearer', token);
                console.log('✅ Authorized via preauthorizeApiKey()');
                return true;
            }

            // Method C: Redux authActions (must use type:'http' not 'apiKey')
            if (ui.authActions && typeof ui.authActions.authorize === 'function') {
                ui.authActions.authorize({
                    Bearer: {
                        name: 'Bearer',
                        schema: {
                            type: 'http',
                            scheme: 'bearer',
                            bearerFormat: 'JWT',
                            in: 'header'
                        },
                        value: token
                    }
                });
                console.log('✅ Authorized via authActions.authorize()');
                return true;
            }

            return false;
        }

        if (window.ui && tryAuthorize(window.ui)) return true;

        if (window.SwaggerUIBundle) {
            const maybeUi = window.SwaggerUIBundle._SwaggerUIStandalone || window.SwaggerUIBundle;
            if (tryAuthorize(maybeUi)) return true;
        }

        // Queue for when UI loads
        localStorage.setItem('authorized', JSON.stringify({
            Bearer: { name: 'Bearer', schema: { type: 'http', scheme: 'bearer' }, value: token }
        }));
        console.warn('⚠️ UI not ready yet; token queued');
        return false;
    }

    // ─── Handle successful login ───────────────────────────────────────────────

    function handleLoginSuccess(token) {
        console.log('🎉 Login successful — token captured');
        saveToken(token);
        window._gymSwaggerAuthorized = false;

        if (!authorizeInSwaggerUI(token)) {
            const delays = [200, 600, 1500, 3000];
            delays.forEach(function (ms) {
                setTimeout(function () {
                    if (!window._gymSwaggerAuthorized) {
                        if (authorizeInSwaggerUI(token)) {
                            window._gymSwaggerAuthorized = true;
                        }
                    }
                }, ms);
            });
        } else {
            window._gymSwaggerAuthorized = true;
        }
    }

    // ─── Fetch interceptor ─────────────────────────────────────────────────────

    function setupFetchInterceptor() {
        var _fetch = window.fetch;

        window.fetch = function () {
            var args = Array.prototype.slice.call(arguments);
            var resource = args[0];
            var config = args[1];

            var isLogin = resource && typeof resource === 'string' &&
                resource.toLowerCase().includes(API_LOGIN_ENDPOINT.toLowerCase());

            var storedToken = getToken();
            if (storedToken && typeof resource === 'string') {
                var headers = Object.assign({}, (config && config.headers) || {});
                if (!headers['Authorization'] && !headers['authorization']) {
                    headers['Authorization'] = 'Bearer ' + storedToken;
                }
                args[1] = Object.assign({}, config || {}, { headers: headers });
            }

            return _fetch.apply(this, args).then(function (response) {
                if (isLogin && response.ok) {
                    var clone = response.clone();
                    clone.json().then(function (data) {
                        console.log('📦 Login response (fetch):', data);
                        var token = extractToken(data);
                        if (token) handleLoginSuccess(token);
                    }).catch(function (e) {
                        console.error('❌ Error reading login response (fetch):', e);
                    });
                }
                return response;
            });
        };

        console.log('🔗 Fetch interceptor ready');
    }

    // ─── XHR interceptor ──────────────────────────────────────────────────────

    function setupXHRInterceptor() {
        var _open = XMLHttpRequest.prototype.open;
        var _send = XMLHttpRequest.prototype.send;

        XMLHttpRequest.prototype.open = function (method, url) {
            this._gymUrl = url;
            return _open.apply(this, arguments);
        };

        XMLHttpRequest.prototype.send = function (body) {
            var self = this;
            var isLogin = self._gymUrl &&
                self._gymUrl.toLowerCase().includes(API_LOGIN_ENDPOINT.toLowerCase());

            if (isLogin) {
                self.addEventListener('load', function () {
                    if (self.status === 200) {
                        try {
                            var data = JSON.parse(self.responseText);
                            console.log('📦 Login response (XHR):', data);
                            var token = extractToken(data);
                            if (token) handleLoginSuccess(token);
                        } catch (e) {
                            console.error('❌ Error reading login response (XHR):', e);
                        }
                    }
                });
            }

            return _send.apply(this, arguments);
        };

        console.log('🔗 XHR interceptor ready');
    }

    // ─── Auto-authorize on page load from stored token ────────────────────────

    function initializeFromStorage() {
        var token = getToken();
        if (!token) {
            console.log('ℹ️ No saved token — log in to authorize automatically');
            return;
        }

        console.log('🔑 Existing token found — restoring authorization...');
        window._gymSwaggerAuthorized = false;

        [500, 1000, 2000, 4000].forEach(function (ms) {
            setTimeout(function () {
                if (!window._gymSwaggerAuthorized) {
                    if (authorizeInSwaggerUI(token)) {
                        window._gymSwaggerAuthorized = true;
                        console.log('✅ Authorization restored from localStorage');
                    }
                }
            }, ms);
        });
    }

    // ─── Public helpers ────────────────────────────────────────────────────────

    window.logoutFromSwagger = function () {
        removeToken();
        window._gymSwaggerAuthorized = false;
        location.reload();
    };

    window.getAuthStatus = function () {
        var token = getToken();
        return {
            isAuthorized: !!token,
            tokenPreview: token ? token.substring(0, 40) + '...' : 'none',
            fullToken: token,
            inLocalStorage: !!localStorage.getItem(TOKEN_KEY)
        };
    };

    window.authorizeManually = function (token) {
        saveToken(token);
        authorizeInSwaggerUI(token);
    };

    // ─── Bootstrap ────────────────────────────────────────────────────────────

    setupFetchInterceptor();
    setupXHRInterceptor();

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeFromStorage);
    } else {
        initializeFromStorage();
    }

    console.log('🚀 Swagger Auto-Auth ready');
    console.log('   getAuthStatus()         — check current status');
    console.log('   logoutFromSwagger()     — clear token & reload');
    console.log('   authorizeManually(tok)  — set token manually');
})();
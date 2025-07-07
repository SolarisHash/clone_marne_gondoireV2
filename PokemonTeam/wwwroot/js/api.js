// === API HELPER FUNCTIONS CORRIGÉ ===

const API = {
    // Configuration
    baseUrl: '',
    game: 'Arena',

    // Generic API call
    async call(endpoint, method = 'GET', data = null) {
        const options = {
            method,
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json'
            }
        };

        if (data && method !== 'GET') {
            options.body = JSON.stringify(data);
        }

        try {
            const response = await fetch(endpoint, options);
            
            // CORRECTION: Gérer les erreurs 401/400 différemment
            if (!response.ok) {
                let errorMessage = `HTTP ${response.status}: ${response.statusText}`;
                
                try {
                    // Essayer de lire comme JSON d'abord
                    const errorData = await response.json();
                    errorMessage = errorData.message || errorData.error || errorMessage;
                } catch {
                    // Si pas JSON, lire comme texte
                    try {
                        const errorText = await response.text();
                        if (errorText) {
                            errorMessage = errorText;
                        }
                    } catch {
                        // Garder le message par défaut
                    }
                }
                
                throw new Error(errorMessage);
            }

            // CORRECTION: Vérifier si la réponse a du contenu
            const contentLength = response.headers.get('content-length');
            const contentType = response.headers.get('content-type');
            
            if (contentLength === '0' || !contentType) {
                return { success: true };
            }
            
            if (contentType && contentType.includes('application/json')) {
                return await response.json();
            }
            
            // Si c'est du texte, le retourner dans un objet
            const textResponse = await response.text();
            return { message: textResponse, success: true };
            
        } catch (error) {
            console.error('API Error:', error);
            throw error;
        }
    },

    // Player endpoints
    player: {
        async getMe() {
            return await API.call(`/api/player/me?game=${API.game}`);
        },

        async create(name) {
            return await API.call('/api/player/create', 'POST', {
                name: name,
                game: API.game
            });
        },

        async update(experience = 0, pokedollar = 0) {
            return await API.call('/api/player/update', 'POST', {
                game: API.game,
                experience: experience,
                pokedollar: pokedollar
            });
        },

        async getItems() {
            return await API.call(`/api/player/my-items?game=${API.game}`);
        },

        async useItem(itemId) {
            return await API.call('/api/player/use-item', 'POST', {
                itemId: itemId,
                game: API.game
            });
        }
    },

    // Shop endpoints
    shop: {
        async getItems() {
            return await API.call('/api/shop/list');
        },

        async buyItem(itemId) {
            return await API.call('/api/shop/buy', 'POST', {
                itemId: itemId,
                game: API.game
            });
        }
    },

    // Combat endpoints
    combat: {
        async useSkill(attacker, target, skill) {
            return await API.call('/Pokemon/UseSkill', 'POST', {
                attacker: attacker,
                target: target,
                skill: skill
            });
        },

        async getTypeMultiplier(attackType, defenderTypes) {
            const params = new URLSearchParams({
                attackType: attackType,
                defenderTypes: defenderTypes
            });
            return await API.call(`/api/TypeChart/multiplier?${params}`);
        }
    },

    // Skills endpoints
    skills: {
        async getAll() {
            return await API.call('/api/Skills');
        },

        async getById(id) {
            return await API.call(`/api/Skills/${id}`);
        }
    },

    // Auth endpoints
    auth: {
        async check() {
            return await API.call('/auth/check');
        },

        async login(email, password) {
            return await API.call('/auth/login', 'POST', {
                email: email,
                password: password
            });
        },

        async logout() {
            return await API.call('/auth/logout', 'POST');
        }
    }
};

// === UTILITY FUNCTIONS ===

// Load player stats and update navbar
async function loadPlayerStats() {
    try {
        const player = await API.player.getMe();
        
        document.getElementById('player-name').textContent = player.name || 'Joueur';
        document.getElementById('player-money').textContent = player.pokedollar || 0;
        document.getElementById('player-xp').textContent = player.experience || 0;
        
        // Store in localStorage for quick access
        localStorage.setItem('currentPlayer', JSON.stringify(player));
        
        return player;
    } catch (error) {
        console.error('Erreur chargement profil:', error);
        
        // Try to create player if not exists
        try {
            const userInfo = await API.auth.check();
            const prefix = userInfo.email.split('@')[0];
            const playerName = `${prefix}-${API.game}`;
            const newPlayer = await API.player.create(playerName);
            
            document.getElementById('player-name').textContent = newPlayer.name;
            document.getElementById('player-money').textContent = newPlayer.pokedollar;
            document.getElementById('player-xp').textContent = newPlayer.experience;
            
            localStorage.setItem('currentPlayer', JSON.stringify(newPlayer));
            return newPlayer;
        } catch (createError) {
            console.error('Erreur création profil:', createError);
            showNotification('Erreur de connexion', 'error');
        }
    }
}

// Show notification
function showNotification(message, type = 'info', duration = 3000) {
    const notification = document.createElement('div');
    notification.className = `alert alert-${type === 'error' ? 'danger' : type === 'success' ? 'success' : 'info'} notification`;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 9999;
        min-width: 300px;
        opacity: 0;
        transition: opacity 0.3s ease;
    `;
    notification.textContent = message;
    
    document.body.appendChild(notification);
    
    // Fade in
    setTimeout(() => notification.style.opacity = '1', 100);
    
    // Fade out and remove
    setTimeout(() => {
        notification.style.opacity = '0';
        setTimeout(() => {
            if (notification.parentElement) {
                document.body.removeChild(notification);
            }
        }, 300);
    }, duration);
}

// Get current player from localStorage
function getCurrentPlayer() {
    const playerData = localStorage.getItem('currentPlayer');
    return playerData ? JSON.parse(playerData) : null;
}

// Format numbers
function formatNumber(num) {
    return new Intl.NumberFormat('fr-FR').format(num);
}

// Random integer between min and max (inclusive)
function randomInt(min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
}

// Random element from array
function randomChoice(array) {
    return array[Math.floor(Math.random() * array.length)];
}

// Clamp value between min and max
function clamp(value, min, max) {
    return Math.min(Math.max(value, min), max);
}

// === POKEMON DATA ===
const POKEMON_DATA = {
    sprites: {
        pikachu: 'https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/25.png',
        bulbasaur: 'https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/1.png',
        charmander: 'https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/4.png',
        squirtle: 'https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/7.png',
        rattata: 'https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/19.png',
        pidgey: 'https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/16.png'
    },
    
    types: ['fire', 'water', 'grass', 'electric', 'normal', 'flying'],
    
    skills: [
        { name: 'Tackle', damage: 40, type: 'normal', powerPoints: 35 },
        { name: 'Thunder Shock', damage: 40, type: 'electric', powerPoints: 30 },
        { name: 'Ember', damage: 40, type: 'fire', powerPoints: 25 },
        { name: 'Water Gun', damage: 40, type: 'water', powerPoints: 25 },
        { name: 'Vine Whip', damage: 45, type: 'grass', powerPoints: 25 }
    ]
};

// === COMBAT HELPERS ===
function createRandomPokemon() {
    const names = ['Rattata', 'Pidgey', 'Weedle', 'Caterpie'];
    const name = randomChoice(names);
    const types = [randomChoice(POKEMON_DATA.types)];
    
    return {
        name: name,
        types: types,
        healthPoint: randomInt(30, 60),
        maxHealthPoint: randomInt(30, 60),
        strength: randomInt(20, 40),
        defense: randomInt(15, 35),
        speed: randomInt(20, 50),
        skills: [randomChoice(POKEMON_DATA.skills)]
    };
}

// === GLOBAL ERROR HANDLER ===
window.addEventListener('error', function(e) {
    console.error('Global error:', e.error);
    showNotification('Une erreur inattendue s\'est produite', 'error');
});

// === INITIALIZATION ===
document.addEventListener('DOMContentLoaded', function() {
    // CORRECTION: Ne pas faire de redirection automatique
    console.log('API initialized');
});
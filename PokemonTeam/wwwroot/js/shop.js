// === SHOP SYSTEM ===

let shopItems = [];
let playerItems = [];

// Load shop items
async function loadShop() {
    try {
        shopItems = await API.shop.getItems();
        displayShopItems();
        updateMoneyDisplay();
    } catch (error) {
        console.error('Erreur utilisation objet:', error);
        showNotification('Erreur lors de l\'utilisation', 'error');
    }
}

// Quick buy buttons with confirmation
function quickBuy(itemName, price) {
    const item = shopItems.find(i => i.name === itemName);
    if (item) {
        buyItem(item.id, item.name, item.price);
    }
}

// Filter shop items
function filterItems(category) {
    const filteredItems = category === 'all' ? 
        shopItems : 
        shopItems.filter(item => getItemCategory(item.name) === category);
    
    displayFilteredItems(filteredItems);
}

// Get item category
function getItemCategory(itemName) {
    if (itemName.toLowerCase().includes('potion')) return 'healing';
    if (itemName.toLowerCase().includes('ball')) return 'pokeball';
    return 'misc';
}

// Display filtered items
function displayFilteredItems(items) {
    const container = document.getElementById('shop-items');
    
    if (items.length === 0) {
        container.innerHTML = `
            <div class="col-12 text-center text-muted">
                <i class="fas fa-search fa-3x mb-3"></i>
                <p>Aucun article dans cette catégorie</p>
            </div>
        `;
        return;
    }
    
    container.innerHTML = items.map(item => `
        <div class="col-md-6 col-lg-4 mb-3">
            <div class="card item-card h-100">
                <div class="card-body text-center">
                    <div class="mb-3">
                        ${getItemIcon(item.name)}
                    </div>
                    <h5 class="card-title">${item.name}</h5>
                    <p class="card-text text-muted">${getItemDescription(item.name)}</p>
                    <div class="price-tag mb-3">
                        <i class="fas fa-coins"></i> ${formatNumber(item.price)} Pokédollars
                    </div>
                    <button class="btn btn-success" onclick="buyItem(${item.id}, '${item.name}', ${item.price})">
                        <i class="fas fa-shopping-cart"></i> Acheter
                    </button>
                </div>
            </div>
        </div>
    `).join('');
}

// Display shop items
function displayShopItems() {
    const container = document.getElementById('shop-items');
    
    if (!shopItems || shopItems.length === 0) {
        container.innerHTML = `
            <div class="col-12 text-center text-muted">
                <i class="fas fa-box-open fa-3x mb-3"></i>
                <p>Aucun article disponible</p>
            </div>
        `;
        return;
    }
    
    container.innerHTML = shopItems.map(item => `
        <div class="col-md-6 col-lg-4 mb-3">
            <div class="card item-card h-100">
                <div class="card-body text-center">
                    <div class="mb-3">
                        ${getItemIcon(item.name)}
                    </div>
                    <h5 class="card-title">${item.name}</h5>
                    <p class="card-text text-muted">${getItemDescription(item.name)}</p>
                    <div class="price-tag mb-3">
                        <i class="fas fa-coins"></i> ${formatNumber(item.price)} Pokédollars
                    </div>
                    <button class="btn btn-success" onclick="buyItem(${item.id}, '${item.name}', ${item.price})">
                        <i class="fas fa-shopping-cart"></i> Acheter
                    </button>
                </div>
            </div>
        </div>
    `).join('');
}

// Get item icon
function getItemIcon(itemName) {
    const icons = {
        'Potion': '<i class="fas fa-flask text-danger fa-3x"></i>',
        'Super Potion': '<i class="fas fa-flask text-primary fa-3x"></i>',
        'Hyper Potion': '<i class="fas fa-flask text-success fa-3x"></i>',
        'Max Potion': '<i class="fas fa-flask text-warning fa-3x"></i>'
    };
    
    return icons[itemName] || '<i class="fas fa-box fa-3x text-secondary"></i>';
}

// Get item description
function getItemDescription(itemName) {
    const descriptions = {
        'Potion': 'Restaure 20 HP à votre Pokémon',
        'Super Potion': 'Restaure 50 HP à votre Pokémon',
        'Hyper Potion': 'Restaure 200 HP à votre Pokémon',
        'Max Potion': 'Restaure tous les HP de votre Pokémon'
    };
    
    return descriptions[itemName] || 'Objet mystérieux...';
}

// Buy item
async function buyItem(itemId, itemName, price) {
    // Check if player has enough money
    const currentPlayer = getCurrentPlayer();
    if (!currentPlayer || currentPlayer.pokedollar < price) {
        showNotification('Vous n\'avez pas assez de Pokédollars !', 'error');
        return;
    }
    
    // Confirm purchase
    if (!confirm(`Acheter ${itemName} pour ${price} Pokédollars ?`)) {
        return;
    }
    
    try {
        const result = await API.shop.buyItem(itemId);
        
        showNotification(`${itemName} acheté avec succès !`, 'success');
        
        // Update displays
        await loadPlayerStats();
        await loadPlayerInventory();
        updateMoneyDisplay();
        
        // Add purchase animation
        animatePurchase(itemName);
        
    } catch (error) {
        console.error('Erreur achat:', error);
        
        if (error.message.includes('400')) {
            showNotification('Vous n\'avez pas assez de Pokédollars !', 'error');
        } else if (error.message.includes('404')) {
            showNotification('Article non trouvé !', 'error');
        } else {
            showNotification('Erreur lors de l\'achat', 'error');
        }
    }
}

// Load player inventory
async function loadPlayerInventory() {
    try {
        playerItems = await API.player.getItems();
        displayPlayerInventory();
    } catch (error) {
        console.error('Erreur chargement inventaire:', error);
        document.getElementById('player-items').innerHTML = `
            <div class="text-center text-danger">
                <i class="fas fa-exclamation-triangle"></i>
                <p>Erreur de chargement</p>
            </div>
        `;
    }
}

// Display player inventory
function displayPlayerInventory() {
    const container = document.getElementById('player-items');
    
    if (!playerItems || playerItems.length === 0) {
        container.innerHTML = `
            <div class="text-center text-muted">
                <i class="fas fa-box-open fa-2x mb-2"></i>
                <p>Inventaire vide</p>
                <small>Achetez des objets !</small>
            </div>
        `;
        return;
    }
    
    // Group items by name and count
    const itemCounts = {};
    playerItems.forEach(item => {
        itemCounts[item.name] = (itemCounts[item.name] || 0) + 1;
    });
    
    container.innerHTML = Object.entries(itemCounts).map(([name, count]) => `
        <div class="inventory-item d-flex justify-content-between align-items-center mb-2 p-2 border rounded">
            <div>
                <small class="text-muted">${getItemIcon(name).replace('fa-3x', 'fa-sm')}</small>
                <strong class="ms-1">${name}</strong>
            </div>
            <span class="badge bg-primary">${count}</span>
        </div>
    `).join('');
}

// Update money display
function updateMoneyDisplay() {
    const currentPlayer = getCurrentPlayer();
    if (currentPlayer) {
        document.getElementById('current-money').textContent = 
            formatNumber(currentPlayer.pokedollar || 0);
    }
}

// Refresh inventory
async function refreshInventory() {
    showNotification('Actualisation...', 'info', 1000);
    await loadPlayerInventory();
}

// Animate purchase
function animatePurchase(itemName) {
    // Create flying coin animation
    const coin = document.createElement('div');
    coin.innerHTML = '<i class="fas fa-coins text-warning fa-2x"></i>';
    coin.style.cssText = `
        position: fixed;
        top: 50%;
        left: 50%;
        z-index: 9999;
        animation: coinFly 1s ease-out forwards;
        pointer-events: none;
    `;
    
    // Add CSS animation
    const style = document.createElement('style');
    style.textContent = `
        @keyframes coinFly {
            0% {
                transform: translate(-50%, -50%) scale(1);
                opacity: 1;
            }
            100% {
                transform: translate(-300px, -200px) scale(0.5);
                opacity: 0;
            }
        }
    `;
    
    document.head.appendChild(style);
    document.body.appendChild(coin);
    
    // Remove elements after animation
    setTimeout(() => {
        document.body.removeChild(coin);
        document.head.removeChild(style);
    }, 1000);
}

// Use item from inventory (future feature)
async function useItem(itemId, itemName) {
    try {
        await API.player.useItem(itemId);
        showNotification(`${itemName} utilisé !`, 'success');
        await loadPlayerInventory();
    } catch (error) {
        console.error('Erreur utilisation objet:', error);
        showNotification('Erreur lors de l\'utilisation', 'error');
    }
}
// === COMBAT SYSTEM ===

let currentBattle = {
    player: null,
    enemy: null,
    isPlayerTurn: true,
    battleActive: false
};

// Initialize combat system
function initializeCombat() {
    setupPlayerPokemon();
    setupEventListeners();
    startNewBattle();
}

// Setup player's Pokémon
function setupPlayerPokemon() {
    currentBattle.player = {
        name: 'Pikachu',
        types: ['electric'],
        healthPoint: 100,
        maxHealthPoint: 100,
        strength: 55,
        defense: 40,
        speed: 90,
        skills: [
            { name: 'Tackle', damage: 40, type: 'normal', powerPoints: 35 },
            { name: 'Thunder Shock', damage: 40, type: 'electric', powerPoints: 30 }
        ]
    };
    
    updatePlayerDisplay();
}

// Setup event listeners
function setupEventListeners() {
    // Skill buttons
    document.querySelectorAll('.skill-button').forEach(button => {
        button.addEventListener('click', function() {
            if (!currentBattle.battleActive || !currentBattle.isPlayerTurn) return;
            
            const skillName = this.dataset.skill;
            const damage = parseInt(this.dataset.damage);
            const type = this.dataset.type;
            
            const skill = {
                name: skillName,
                damage: damage,
                type: type,
                powerPoints: 10
            };
            
            playerAttack(skill);
        });
    });
    
    // New battle button
    document.getElementById('new-battle-btn').addEventListener('click', startNewBattle);
    
    // Continue button
    document.getElementById('continue-btn').addEventListener('click', startNewBattle);
    
    // Run button
    document.getElementById('run-btn').addEventListener('click', runFromBattle);
}

// Start a new battle
function startNewBattle() {
    // Generate random enemy
    currentBattle.enemy = generateRandomEnemy();
    currentBattle.isPlayerTurn = true;
    currentBattle.battleActive = true;
    
    // Reset player health
    currentBattle.player.healthPoint = currentBattle.player.maxHealthPoint;
    
    updateDisplays();
    hideResultScreen();
    
    logMessage(`Un ${currentBattle.enemy.name} sauvage apparaît !`);
    logMessage('À vous de jouer !');
}

// Generate random enemy Pokémon
function generateRandomEnemy() {
    const enemies = [
        { name: 'Rattata', types: ['normal'], sprite: '19' },
        { name: 'Pidgey', types: ['normal', 'flying'], sprite: '16' },
        { name: 'Weedle', types: ['bug', 'poison'], sprite: '13' },
        { name: 'Caterpie', types: ['bug'], sprite: '10' }
    ];
    
    const template = randomChoice(enemies);
    
    return {
        name: template.name,
        types: template.types,
        healthPoint: randomInt(40, 80),
        maxHealthPoint: randomInt(40, 80),
        strength: randomInt(25, 45),
        defense: randomInt(20, 40),
        speed: randomInt(30, 60),
        sprite: template.sprite,
        skills: [
            { name: 'Tackle', damage: randomInt(30, 50), type: 'normal', powerPoints: 20 }
        ]
    };
}

// Player attacks
async function playerAttack(skill) {
    if (!currentBattle.battleActive) return;
    
    currentBattle.isPlayerTurn = false;
    disableActions();
    
    try {
        // Calculate damage with type effectiveness
        let damage = calculateDamage(
            currentBattle.player,
            currentBattle.enemy,
            skill
        );
        
        // Apply damage
        currentBattle.enemy.healthPoint -= damage;
        currentBattle.enemy.healthPoint = Math.max(0, currentBattle.enemy.healthPoint);
        
        // Log attack
        logMessage(`${currentBattle.player.name} utilise ${skill.name} !`);
        
        // Show damage animation
        showDamageEffect(damage, 'enemy');
        
        // Update displays
        updateEnemyDisplay();
        
        // Check if enemy is defeated
        if (currentBattle.enemy.healthPoint <= 0) {
            setTimeout(() => {
                playerWins();
            }, 1000);
            return;
        }
        
        logMessage(`${currentBattle.enemy.name} subît ${damage} points de dégâts !`);
        
        // Enemy turn after delay
        setTimeout(() => {
            enemyAttack();
        }, 1500);
        
    } catch (error) {
        console.error('Erreur pendant l\'attaque:', error);
        logMessage('Erreur pendant l\'attaque !');
        currentBattle.isPlayerTurn = true;
        enableActions();
    }
}

// Enemy attacks
function enemyAttack() {
    if (!currentBattle.battleActive) return;
    
    const enemySkill = randomChoice(currentBattle.enemy.skills);
    
    // Calculate damage
    let damage = calculateDamage(
        currentBattle.enemy,
        currentBattle.player,
        enemySkill
    );
    
    // Apply damage
    currentBattle.player.healthPoint -= damage;
    currentBattle.player.healthPoint = Math.max(0, currentBattle.player.healthPoint);
    
    // Log attack
    logMessage(`${currentBattle.enemy.name} utilise ${enemySkill.name} !`);
    
    // Show damage animation
    showDamageEffect(damage, 'player');
    
    // Update displays
    updatePlayerDisplay();
    
    // Check if player is defeated
    if (currentBattle.player.healthPoint <= 0) {
        setTimeout(() => {
            playerLoses();
        }, 1000);
        return;
    }
    
    logMessage(`${currentBattle.player.name} subît ${damage} points de dégâts !`);
    
    // Player turn
    setTimeout(() => {
        currentBattle.isPlayerTurn = true;
        enableActions();
    }, 1500);
}

// Calculate damage between attacker and defender
function calculateDamage(attacker, defender, skill) {
    // Base damage calculation
    let damage = Math.floor(
        (skill.damage * (attacker.strength / defender.defense)) * 
        (0.8 + Math.random() * 0.4) // Random factor 0.8-1.2
    );
    
    // Type effectiveness (simplified)
    let typeMultiplier = 1.0;
    
    if (skill.type === 'electric' && defender.types.includes('water')) {
        typeMultiplier = 2.0;
    } else if (skill.type === 'water' && defender.types.includes('fire')) {
        typeMultiplier = 2.0;
    } else if (skill.type === 'fire' && defender.types.includes('grass')) {
        typeMultiplier = 2.0;
    } else if (skill.type === 'grass' && defender.types.includes('water')) {
        typeMultiplier = 2.0;
    }
    
    damage = Math.floor(damage * typeMultiplier);
    
    // Minimum damage
    return Math.max(1, damage);
}

// Player wins the battle
async function playerWins() {
    currentBattle.battleActive = false;
    
    logMessage(`${currentBattle.enemy.name} est KO !`);
    logMessage('Vous remportez le combat !');
    
    // Calculate rewards
    const xpGain = randomInt(15, 35);
    const moneyGain = randomInt(10, 25);
    
    try {
        // Update player stats
        await API.player.update(xpGain, moneyGain);
        await loadPlayerStats();
        
        showResultScreen(true, xpGain, moneyGain);
        
        logMessage(`Vous gagnez ${xpGain} XP et ${moneyGain} Pokédollars !`);
        
    } catch (error) {
        console.error('Erreur mise à jour stats:', error);
        showResultScreen(true, xpGain, moneyGain);
    }
}

// Player loses the battle
function playerLoses() {
    currentBattle.battleActive = false;
    
    logMessage(`${currentBattle.player.name} est KO !`);
    logMessage('Vous perdez le combat...');
    
    showResultScreen(false, 0, 0);
}

// Run from battle
function runFromBattle() {
    if (!currentBattle.battleActive) return;
    
    currentBattle.battleActive = false;
    logMessage('Vous fuyez le combat !');
    
    setTimeout(() => {
        startNewBattle();
    }, 1000);
}

// Show damage effect
function showDamageEffect(damage, target) {
    const targetElement = target === 'player' ? 
        document.getElementById('player-pokemon-sprite') : 
        document.getElementById('enemy-pokemon-sprite');
    
    // Shake animation
    targetElement.classList.add('shake');
    setTimeout(() => {
        targetElement.classList.remove('shake');
    }, 500);
    
    // Damage number
    const damageText = document.createElement('div');
    damageText.className = 'damage-text';
    damageText.textContent = `-${damage}`;
    damageText.style.left = (targetElement.offsetLeft + 50) + 'px';
    damageText.style.top = (targetElement.offsetTop + 30) + 'px';
    
    targetElement.parentElement.appendChild(damageText);
    
    setTimeout(() => {
        if (damageText.parentElement) {
            damageText.parentElement.removeChild(damageText);
        }
    }, 1000);
}

// Update player display
function updatePlayerDisplay() {
    const pokemon = currentBattle.player;
    
    document.getElementById('player-pokemon-name').textContent = pokemon.name;
    document.getElementById('player-pokemon-sprite').src = 
        `https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/25.png`;
    
    const healthPercent = (pokemon.healthPoint / pokemon.maxHealthPoint) * 100;
    document.getElementById('player-health-fill').style.width = healthPercent + '%';
    document.getElementById('player-health-text').textContent = 
        `${pokemon.healthPoint}/${pokemon.maxHealthPoint} HP`;
    
    document.getElementById('player-attack').textContent = pokemon.strength;
    document.getElementById('player-defense').textContent = pokemon.defense;
}

// Update enemy display
function updateEnemyDisplay() {
    const pokemon = currentBattle.enemy;
    
    document.getElementById('enemy-pokemon-name').textContent = pokemon.name;
    document.getElementById('enemy-pokemon-sprite').src = 
        `https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/${pokemon.sprite}.png`;
    
    const healthPercent = (pokemon.healthPoint / pokemon.maxHealthPoint) * 100;
    document.getElementById('enemy-health-fill').style.width = healthPercent + '%';
    document.getElementById('enemy-health-text').textContent = 
        `${pokemon.healthPoint}/${pokemon.maxHealthPoint} HP`;
    
    document.getElementById('enemy-attack').textContent = pokemon.strength;
    document.getElementById('enemy-defense').textContent = pokemon.defense;
}

// Update both displays
function updateDisplays() {
    updatePlayerDisplay();
    updateEnemyDisplay();
}

// Show result screen
function showResultScreen(victory, xp, money) {
    const resultElement = document.getElementById('battle-result');
    const rewardsElement = document.getElementById('rewards');
    
    if (victory) {
        resultElement.textContent = 'VICTOIRE !';
        resultElement.className = 'text-success';
        rewardsElement.innerHTML = `
            <div class="alert alert-success">
                <i class="fas fa-trophy"></i> Récompenses :<br>
                <strong>+${xp} XP</strong><br>
                <strong>+${money} Pokédollars</strong>
            </div>
        `;
    } else {
        resultElement.textContent = 'DÉFAITE...';
        resultElement.className = 'text-danger';
        rewardsElement.innerHTML = `
            <div class="alert alert-warning">
                <i class="fas fa-skull"></i> Aucune récompense cette fois...
            </div>
        `;
    }
    
    document.getElementById('combat-actions').style.display = 'none';
    document.getElementById('battle-over').style.display = 'block';
}

// Hide result screen
function hideResultScreen() {
    document.getElementById('combat-actions').style.display = 'block';
    document.getElementById('battle-over').style.display = 'none';
    enableActions();
}

// Disable combat actions
function disableActions() {
    document.querySelectorAll('.skill-button').forEach(btn => {
        btn.disabled = true;
    });
}

// Enable combat actions
function enableActions() {
    document.querySelectorAll('.skill-button').forEach(btn => {
        btn.disabled = false;
    });
}

// Add message to combat log
function logMessage(message) {
    const log = document.getElementById('combat-log');
    const timestamp = new Date().toLocaleTimeString();
    
    log.innerHTML += `<div>[${timestamp}] ${message}</div>`;
    log.scrollTop = log.scrollHeight;
}

// Use potion
function usePotion() {
    if (!currentBattle.battleActive || !currentBattle.isPlayerTurn) return;
    
    const healAmount = 50;
    const oldHealth = currentBattle.player.healthPoint;
    
    currentBattle.player.healthPoint = Math.min(
        currentBattle.player.maxHealthPoint,
        currentBattle.player.healthPoint + healAmount
    );
    
    const actualHeal = currentBattle.player.healthPoint - oldHealth;
    
    if (actualHeal > 0) {
        updatePlayerDisplay();
        logMessage(`Vous utilisez une Potion ! +${actualHeal} HP`);
        
        // End turn
        currentBattle.isPlayerTurn = false;
        disableActions();
        
        setTimeout(() => {
            enemyAttack();
        }, 1500);
    } else {
        logMessage('Votre Pokémon a déjà tous ses HP !');
    }
}

// Show items (placeholder)
function showItems() {
    showNotification('Inventaire - Fonctionnalité à venir !', 'info');
}
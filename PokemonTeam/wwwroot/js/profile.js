// === PROFILE SYSTEM ===

let playerProfile = null;

// Load player profile
async function loadProfile() {
    try {
        playerProfile = await API.player.getMe();
        displayProfile();
        updateAchievements();
        addRecentActivity('Profil consulté');
    } catch (error) {
        console.error('Erreur chargement profil:', error);
        showNotification('Erreur de chargement du profil', 'error');
    }
}

// Display player profile
function displayProfile() {
    if (!playerProfile) return;
    
    // Basic info
    document.getElementById('profile-name').textContent = playerProfile.name || 'Dresseur';
    document.getElementById('profile-xp').textContent = formatNumber(playerProfile.experience || 0);
    document.getElementById('profile-money').textContent = formatNumber(playerProfile.pokedollar || 0);
    
    // Calculate level
    const level = calculateLevel(playerProfile.experience || 0);
    document.getElementById('profile-level').textContent = `Niveau ${level}`;
    
    // Calculate battles won (simplified - based on XP)
    const battlesWon = Math.floor((playerProfile.experience || 0) / 25);
    document.getElementById('profile-battles').textContent = formatNumber(battlesWon);
    
    // Update XP progress bar
    updateXPProgress();
}

// Calculate player level
function calculateLevel(xp) {
    return Math.floor(xp / 100) + 1;
}

// Update XP progress bar
function updateXPProgress() {
    const currentXP = playerProfile.experience || 0;
    const currentLevel = calculateLevel(currentXP);
    const xpForCurrentLevel = (currentLevel - 1) * 100;
    const xpForNextLevel = currentLevel * 100;
    const progressXP = currentXP - xpForCurrentLevel;
    const neededXP = xpForNextLevel - xpForCurrentLevel;
    
    const progressPercent = (progressXP / neededXP) * 100;
    
    document.getElementById('xp-progress').style.width = progressPercent + '%';
    document.getElementById('xp-progress-text').textContent = `${progressXP} / ${neededXP} XP`;
}

// Update achievements based on player stats
function updateAchievements() {
    if (!playerProfile) return;
    
    const xp = playerProfile.experience || 0;
    const money = playerProfile.pokedollar || 0;
    const level = calculateLevel(xp);
    const battlesWon = Math.floor(xp / 25);
    
    // Achievement conditions
    const achievements = [
        {
            id: 'first-steps',
            condition: true, // Always unlocked when profile exists
            icon: 'fas fa-baby',
            title: 'Premier Pas',
            description: 'Créer votre profil'
        },
        {
            id: 'first-battle',
            condition: battlesWon >= 1,
            icon: 'fas fa-sword',
            title: 'Premier Combat',
            description: 'Gagner votre premier combat'
        },
        {
            id: 'rich-trainer',
            condition: money >= 1000,
            icon: 'fas fa-coins',
            title: 'Collectionneur',
            description: 'Gagner 1000 Pokédollars'
        },
        {
            id: 'master-trainer',
            condition: level >= 10,
            icon: 'fas fa-crown',
            title: 'Maître Dresseur',
            description: 'Atteindre le niveau 10'
        },
        {
            id: 'shopaholic',
            condition: money >= 500, // Simplified check
            icon: 'fas fa-shopping-bag',
            title: 'Acheteur Compulsif',
            description: 'Dépenser dans la boutique'
        }
    ];
    
    const container = document.getElementById('achievements');
    container.innerHTML = achievements.map(achievement => `
        <div class="achievement-item ${achievement.condition ? 'unlocked' : 'locked'}">
            <i class="${achievement.icon} ${achievement.condition ? 'text-warning' : 'text-muted'}"></i>
            <div>
                <strong>${achievement.title}</strong>
                <small>${achievement.description}</small>
            </div>
            <i class="fas ${achievement.condition ? 'fa-check-circle text-success' : 'fa-lock text-muted'}"></i>
        </div>
    `).join('');
}

// Add activity to recent log
function addRecentActivity(activity) {
    const container = document.getElementById('recent-activity');
    const timestamp = new Date().toLocaleTimeString();
    
    const activityItem = document.createElement('div');
    activityItem.className = 'activity-item';
    activityItem.innerHTML = `
        <i class="fas fa-clock text-info"></i>
        <span>${activity}</span>
        <small class="text-muted">${timestamp}</small>
    `;
    
    // Add to top of list
    container.insertBefore(activityItem, container.firstChild);
    
    // Keep only last 5 activities
    const items = container.querySelectorAll('.activity-item');
    if (items.length > 5) {
        container.removeChild(items[items.length - 1]);
    }
    
    // Store in localStorage
    const activities = getStoredActivities();
    activities.unshift({
        text: activity,
        time: timestamp,
        date: new Date().toDateString()
    });
    
    // Keep only last 10
    if (activities.length > 10) {
        activities.splice(10);
    }
    
    localStorage.setItem('recentActivities', JSON.stringify(activities));
}

// Get stored activities
function getStoredActivities() {
    const stored = localStorage.getItem('recentActivities');
    return stored ? JSON.parse(stored) : [];
}

// Load stored activities
function loadStoredActivities() {
    const activities = getStoredActivities();
    const container = document.getElementById('recent-activity');
    
    container.innerHTML = activities.slice(0, 5).map(activity => `
        <div class="activity-item">
            <i class="fas fa-clock text-info"></i>
            <span>${activity.text}</span>
            <small class="text-muted">${activity.time}</small>
        </div>
    `).join('');
}

// Refresh profile
async function refreshProfile() {
    showNotification('Actualisation du profil...', 'info', 1000);
    await loadPlayerStats();
    await loadProfile();
    showNotification('Profil actualisé !', 'success');
}

// Reset progress (debug function)
async function resetProgress() {
    if (!confirm('Êtes-vous sûr de vouloir réinitialiser votre progression ? Cette action est irréversible !')) {
        return;
    }
    
    try {
        // This would require a backend endpoint to reset player data
        // For now, just clear localStorage
        localStorage.removeItem('recentActivities');
        localStorage.removeItem('currentPlayer');
        
        showNotification('Progression réinitialisée ! Rechargement...', 'info');
        
        setTimeout(() => {
            window.location.reload();
        }, 2000);
        
    } catch (error) {
        console.error('Erreur réinitialisation:', error);
        showNotification('Erreur lors de la réinitialisation', 'error');
    }
}

// Get player statistics summary
function getPlayerStats() {
    if (!playerProfile) return null;
    
    const xp = playerProfile.experience || 0;
    const money = playerProfile.pokedollar || 0;
    const level = calculateLevel(xp);
    const battlesWon = Math.floor(xp / 25);
    const itemsBought = Math.floor(money / 100); // Simplified calculation
    
    return {
        level,
        xp,
        money,
        battlesWon,
        itemsBought,
        joinDate: new Date().toLocaleDateString() // Simplified
    };
}

// Export profile data (for sharing/backup)
function exportProfile() {
    const stats = getPlayerStats();
    const activities = getStoredActivities();
    
    const exportData = {
        profile: playerProfile,
        stats,
        activities,
        exportDate: new Date().toISOString()
    };
    
    const dataStr = JSON.stringify(exportData, null, 2);
    const dataBlob = new Blob([dataStr], { type: 'application/json' });
    
    const link = document.createElement('a');
    link.href = URL.createObjectURL(dataBlob);
    link.download = `pokemon-profile-${playerProfile.name || 'player'}.json`;
    link.click();
    
    showNotification('Profil exporté !', 'success');
}

// Initialize profile page
document.addEventListener('DOMContentLoaded', function() {
    loadStoredActivities();
});

// Add some sample achievements for testing
function unlockAchievement(achievementId) {
    const achievement = document.querySelector(`[data-achievement="${achievementId}"]`);
    if (achievement) {
        achievement.classList.remove('locked');
        achievement.classList.add('unlocked');
        showNotification('Nouvel achievement débloqué !', 'success');
    }
}
const urlParams = new URLSearchParams(window.location.search);

fetch('/auth/check', { credentials: 'include' })
    .then(res => {
        if (res.status === 200) {
            return res.json();
        } else {
            throw new Error('Not logged in');
        }
    })
    .then(data => {
        document.getElementById('auth-buttons').style.display = 'none';
        document.getElementById('logout-section').style.display = 'block';
        document.getElementById('player-info').innerText = `👋 ${data.email}`;
    })
    .catch(err => {
        console.log('Pas connecté');
    });

document.getElementById('logout-btn').addEventListener('click', async () => {
    await fetch('/auth/logout', {
        method: 'POST',
        credentials: 'include'
    });
    window.location.href = '/';
});

if (game) {
// Récupère l'utilisateur connecté
fetch('/auth/check', { credentials: 'include' })
    .then(res => res.ok ? res.json() : Promise.reject('Not logged'))
    .then(async data => {
        const email = data.email;

        // Vérifie si le Player existe pour ce jeu
        const resPlayer = await fetch(`/api/player/me?game=${game}`, { credentials: 'include' });

        if (resPlayer.ok) {
            const player = await resPlayer.json();
            document.getElementById('player-info').innerText = `👋 ${player.name}`;
        } else {
            // Crée le Player automatiquement
            const prefix = email.split('@@')[0];
            const playerName = `${prefix}-${game}`;

            const createRes = await fetch('/api/player/create', {
                method: 'POST',
                credentials: 'include',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ name: playerName, game: game })
            });

            if (createRes.ok) {
                const newPlayer = await createRes.json();
                document.getElementById('player-info').innerText = `👋 ${newPlayer.name}`;
            } else {
                console.error("Erreur création profil joueur");
            }
        }
    })
    .catch(err => console.log('Utilisateur non connecté'));
}
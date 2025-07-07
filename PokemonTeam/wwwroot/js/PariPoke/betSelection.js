// betSelection.js

console.log("BetSelection.js chargé ✔️");

document.addEventListener("DOMContentLoaded", () => {

    const betContainer = document.getElementById("betSelection");

    document.getElementById("confirmRunners").addEventListener("click", () => {
        if (selectedRunners.length !== 4) {
            alert("Choisis exactement 4 Pokémon coureurs avant de parier !");
            return;
        }

        betContainer.innerHTML = ""; // Clear

        selectedRunners.forEach(pokeName => {
            const card = document.createElement('div');
            card.className = 'pokemon-card';
            card.innerHTML = `<p>${pokeName}</p>`;

            card.addEventListener('click', () => {
                document.querySelectorAll('#betSelection .pokemon-card').forEach(c => c.classList.remove('selected'));
                card.classList.add('selected');
                document.getElementById('selectedBetPokemon').value = pokeName;
            });

            betContainer.appendChild(card);
        });

        betContainer.style.display = 'flex';
    });

    document.getElementById("confirmBet").addEventListener("click", () => {
        const selectedBetPokemon = document.getElementById("selectedBetPokemon").value;
        const betAmount = parseInt(document.getElementById("betAmount").value, 10);

        if (!selectedBetPokemon) {
            alert("Choisis un Pokémon sur lequel parier !");
            return;
        }

        if (isNaN(betAmount) || betAmount <= 0) {
            alert("Entre un montant valide pour ton pari !");
            return;
        }

        console.log("✅ Pari validé :");
        console.log("Pokémon :", selectedBetPokemon);
        console.log("Montant :", betAmount);

        // Ici tu peux :
        // - stocker en localStorage
        // - envoyer à ton startRace
        // - déclencher le lancement de la course
    });
});

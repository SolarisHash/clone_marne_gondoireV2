// runnersSelection.js

console.log("RunnerSelection.js chargé ✔️");

// Ta liste globale de pokémons possibles
const allPokemons = [
    "pikachu", "bulbasaur", "charmander", "squirtle",
    "eevee", "snorlax", "jigglypuff", "meowth",
    "psyduck", "gengar", "mewtwo", "mew",
    "pidgey", "rattata", "zubat", "machop",
    "abra", "magikarp", "vulpix", "growlithe"
];

const runnersContainer = document.getElementById("runnersSelection");
const selectedRunners = [];

allPokemons.forEach(async pokeName => {
    const res = await fetch(`https://pokeapi.co/api/v2/pokemon/${pokeName}`);
    const data = await res.json();
    const frontSprite = data.sprites.front_default;

    const card = document.createElement('div');
    card.className = 'pokemon-card';
    card.innerHTML = `
        <img src="${frontSprite}" alt="${pokeName}" />
        <p>${pokeName}</p>
    `;

    card.addEventListener('click', () => {
        if (card.classList.contains('selected')) {
            card.classList.remove('selected');
            const index = selectedRunners.indexOf(pokeName);
            if (index > -1) selectedRunners.splice(index, 1);
        } else {
            if (selectedRunners.length >= 4) {
                alert("Tu peux sélectionner maximum 4 Pokémon !");
                return;
            }
            card.classList.add('selected');
            selectedRunners.push(pokeName);
        }
    });

    runnersContainer.appendChild(card);
});

// Gestion bouton valider
document.getElementById("confirmRunners").addEventListener("click", () => {
    if (selectedRunners.length !== 4) {
        alert("Choisis exactement 4 Pokémon avant de valider !");
        return;
    }
    console.log("Tes coureurs sont :", selectedRunners);
});

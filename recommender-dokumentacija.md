# Sistem preporuke — Happenings App

## Pregled

Happenings aplikacija koristi **Collaborative Filtering** pristup zasnovan na
**Singular Value Decomposition (SVD)** algoritmu (Matrix Factorization) za
generisanje personalizovanih preporuka događaja korisnicima.

## Tip algoritma

**Matrix Factorization — Truncated SVD (Singular Value Decomposition)**

SVD je tehnika iz linearne algebre koja u sistemima preporuke otkriva latentne
faktore u podacima o interakcijama korisnika i stavki (ovdje: korisnika i
događaja). Koristi se **skraćeni (truncated)** SVD — zadržava se samo mali broj
najjačih latentnih faktora kako bi model generalizovao i procijenio interakcije
i za parove koje korisnik nije direktno ostvario.

## Podaci koji ulaze u recommender

Sistem prikuplja tri vrste signala interakcija koji se stvarno upisuju u aplikaciji:

| Signal | Vrijednost | Opis |
|--------|-----------|------|
| **Rezervacija** | 5 | Korisnik je rezervisao kartu za događaj |
| **Recenzija** | stvarni rating (1–5) | Korisnik je ostavio ocjenu za događaj |
| **Pregled** | 3 | Korisnik je pregledao stranicu događaja |

Ovi signali se bilježe u tabelama `Reservations`, `Reviews` i `EventViews`.
Pregled (`EventView`) se upisuje kada korisnik otvori detalje događaja u mobilnoj
aplikaciji.

## Matrica interakcija

Gradi se matrica dimenzija **M × N**, gdje je **M** broj korisnika, a **N** broj
**budućih** događaja (prošli događaji se filtriraju). Element `matrix[i][j]`
predstavlja jačinu interakcije između korisnika `i` i događaja `j`. Za isti par
korisnik–događaj koristi se **maksimalna** vrijednost od svih signala (npr. ako
je korisnik i pregledao i rezervisao događaj, uzima se 5).

## Algoritam

### Korak 1 — Izgradnja matrice interakcija
Matrica se popunjava na osnovu rezervacija (5), pregleda (3) i recenzija
(stvarni rating 1–5), uzimajući maksimum po paru korisnik–događaj.

### Korak 2 — Cold start
Ako korisnik nema nijednu interakciju (novi korisnik), vraća se **6
najpopularnijih budućih događaja** po broju rezervacija (popularity-based
fallback), uz objašnjenje „Popular event".

### Korak 3 — Truncated SVD dekompozicija
Matrica se dekomponuje na `M = U × Σ × Vᵀ`, gdje je **U** matrica korisničkih
faktora, **Σ** dijagonalna matrica singularnih vrijednosti, a **Vᵀ** matrica
faktora događaja. Zadržava se samo **top-k** singularnih vrijednosti, gdje je
`k = min(rang − 1, 10)` (rang = min(M, N)). Time se odbacuju slabiji/šum faktori
i osigurava da `k` bude manje od punog ranga (inače bi rekonstrukcija samo
vratila ulaznu matricu).

### Korak 4 — Rekonstrukcija i scoring
Rekonstruisana matrica `U × Σₖ × Vᵀ` sadrži procijenjene vrijednosti interakcija
za sve parove korisnik–događaj, uključujući i one koji nisu direktno zabilježeni.
Procijenjena vrijednost za ciljnog korisnika i kandidat-događaj je njegov **score**.

### Korak 5 — Generisanje preporuka
Za ciljnog korisnika:
1. Isključuju se događaji s kojima je već interagovao (rezervacija/pregled/recenzija).
2. Preostali događaji se sortiraju opadajuće po procijenjenom score-u.
3. Vraća se **top 6** preporuka.

## Objašnjenje preporuka (explainable)

Svaka preporuka nosi `Reason` polje koje se prikazuje u mobilnoj aplikaciji.
Logika objašnjenja (`GenerateReason`):

| Uslov | Objašnjenje |
|-------|-------------|
| prosječna ocjena događaja ≥ 4 | `Highly rated (X.X★)` |
| > 10 rezervacija | `Popular — N reservations` |
| score > 3 | `Based on your interests` |
| inače | `Recommended for you` |

Score direktno odgovara rekonstruisanoj vrijednosti iz (truncated) SVD matrice —
viši score znači veću vjerovatnoću da će se događaj svidjeti korisniku, na osnovu
obrazaca interakcija sličnih korisnika.

## Tehnologije
- **MathNet.Numerics** — SVD dekompozicija u C#
- **Entity Framework Core** — dohvat podataka iz SQL Server baze

## Ograničenja
- SVD zahtijeva dovoljno podataka za kvalitetne preporuke.
- Za nove korisnike bez historije koristi se popularity-based fallback.
- Sistem ne koristi content-based filtriranje (karakteristike događaja).

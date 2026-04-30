\# Sistem preporuke — Happenings App



\## Pregled



Happenings aplikacija koristi \*\*Collaborative Filtering\*\* pristup zasnovan na \*\*Singular Value Decomposition (SVD)\*\* algoritmu za generisanje personalizovanih preporuka događaja korisnicima.



\## Tip algoritma



\*\*Matrix Factorization (SVD — Singular Value Decomposition)\*\*



SVD je tehnika iz linearne algebre koja se koristi u sistemima preporuke za otkrivanje latentnih faktora u podacima o interakcijama korisnika i stavki (u ovom slučaju, korisnika i događaja).



\## Podaci koji ulaze u recommender



Sistem prikuplja tri vrste signala interakcija koji se stvarno upisuju u aplikaciji:



| Signal | Vrijednost | Opis |

|--------|-----------|------|

| \*\*Rezervacija\*\* | 5 | Korisnik je rezervisao kartu za događaj |

| \*\*Recenzija\*\* | 4 | Korisnik je ostavio ocjenu za događaj |

| \*\*Pregled\*\* | 3 | Korisnik je pregledao stranicu događaja |



Ovi signali se bilježe u tabelama `Reservations`, `Reviews` i `EventViews` u bazi podataka.



\## Matrica interakcija



Gradi se matrica dimenzija \*\*M × N\*\*, gdje:

\- \*\*M\*\* = broj korisnika u sistemu

\- \*\*N\*\* = broj događaja u sistemu



Svaki element matrice `matrix\[i]\[j]` predstavlja jačinu interakcije između korisnika `i` i događaja `j`. Koristi se maksimalna vrijednost od svih signala za isti par korisnik-događaj.



\## Algoritam



\### Korak 1 — Izgradnja matrice interakcija

Za svakog korisnika i svaki događaj popunjava se matrica na osnovu rezervacija, pregleda i recenzija.



\### Korak 2 — Cold Start

Ako korisnik nema nijednu interakciju (novi korisnik), vraća se \*\*5 najpopularnijih događaja\*\* po broju rezervacija — popularity-based fallback.



\### Korak 3 — SVD dekompozicija

Matrica se dekomponiuje na tri matrice:

```

M = U × Σ × V^T

```

gdje:

\- \*\*U\*\* — matrica korisničkih faktora

\- \*\*Σ\*\* — dijagonalna matrica singularnih vrijednosti

\- \*\*V^T\*\* — matrica faktora događaja



\### Korak 4 — Rekonstrukcija i scoring

Rekonstruisana matrica `U × Σ × V^T` sadrži procijenjene vrijednosti interakcija za sve parove korisnik-događaj, uključujući i one koji nisu direktno zabilježeni.



\### Korak 5 — Generisanje preporuka

Za ciljnog korisnika:

1\. Isključuju se događaji s kojima je korisnik već interagovao

2\. Preostali događaji se sortiraju po procijenjenom score-u

3\. Vraća se \*\*top 5\*\* preporuka



\## Objašnjenje preporuka



Sistem preporučuje događaje na osnovu sličnosti između korisnika — ako su korisnik A i korisnik B imali slične interakcije (pregledali i rezervisali iste događaje), sistem pretpostavlja da će im se svidjeti slični događaji u budućnosti.



Score koji se dodjeljuje događaju direktno odgovara rekonstruisanoj vrijednosti iz SVD matrice — viši score znači veća vjerojatnoća da će se korisniku svidjeti taj događaj, na osnovu obrazaca interakcija sličnih korisnika.



\## Tehnologije



\- \*\*MathNet.Numerics\*\* — biblioteka za SVD dekompoziciju u C#

\- \*\*Entity Framework Core\*\* — dohvat podataka iz SQL Server baze



\## Ograničenja



\- SVD zahtijeva dovoljno podataka za kvalitetne preporuke

\- Za nove korisnike bez historije koristi se popularity-based fallback

\- Sistem ne koristi content-based filtriranje (karakteristike događaja)


# 🏫 KeelteKooli

**KeelteKooli** on keelekooli infosüsteem, mis on loodud õpilaste, õpetajate, kursuste ja muu kooliga seotud info haldamiseks.

Projekt sisaldab C# klasse, andmebaasiühendust ja **Entity Framework** migratsioone.

## 🎯 Projekti eesmärk

* Keelekooli andmete haldamine
* Õpilaste ja õpetajate info salvestamine
* Kursuste ja õppetööga seotud andmete haldamine
* Andmebaasi kasutamine Entity Frameworki abil

## 🛠️ Kasutatud tehnoloogiad

| Tehnoloogia      | Kasutus                          |
| ---------------- | -------------------------------- |
| C#               | Rakenduse arendus                |
| .NET             | Projekti platvorm                |
| Entity Framework | Andmebaasi haldamine             |
| SQL Server       | Andmebaas                        |
| Migrations       | Andmebaasi struktuuri uuendamine |

## ⚙️ Projekti arendus

Projektis loodi erinevad klassid, mis kirjeldavad keelekooli andmeid ja nende omavahelisi seoseid.

Entity Frameworki abil loodi andmebaasi mudel ning migrations võimaldavad andmebaasi struktuuri muuta ja uuendada.

## 📁 Projekti struktuur

```text
KeelteKooli/
├── Models/             # Andmemudelid ja klassid
├── Migrations/         # Entity Framework migratsioonid
├── Data/               # Andmebaasi konfiguratsioon
```

## ▶️ Projekti käivitamine

1. Ava projekt Visual Studios.
2. Sea ühendus andmebaasiga.
3. Uuenda andmebaasi migrations abil.
4. Käivita projekt.

Andmebaasi uuendamiseks:

```powershell
Update-Database
```

## 👨‍💻 Projekt

Projekt on loodud õppetöö raames C# ja Entity Frameworki praktiliseks kasutamiseks.

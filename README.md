# Industrial IoT, Agent OPC UA -> Azure IoT Hub

Agent pełni funkcję mostu komunikacyjnego między urządzeniem produkcyjnym dostępnym przez **OPC UA** a **Azure IoT Hub**.  
Każde fizyczne urządzenie jest mapowane na jedno urządzenie w IoT Hub.

---

## 1. Konfiguracja i Połączenie

1. Dodaj **Primary Connection String** swojego urządzenia IoT do pliku: ``Config.txt``
2. Uruchom agenta, wyświetli on listę dostępnych urządzeń z serwera OPC UA.
3. Wybierz urządzenie, które chcesz połączyć z IoT Hub.

---

## 2. Telemetria

Po ustanowieniu połączenia agent wysyła dane telemetryczne do IoT Hub co **1 sekundę**:

| Pole            | Opis |
|-----------------|------|
| **ProductionStatus** | Aktualny status pracy urządzenia<br>• `1` - Włączony<br>• `0` - Wyłączony |
| **WorkorderID** | Unikalny identyfikator urządzenia |
| **GoodCount**   | Liczba poprawnie wykonanych produktów |
| **BadCount**    | Liczba wadliwych produktów |
| **Temperature** | Aktualna temperatura urządzenia |

Przykład telemetrii:

```json
{
  "ProductionStatus": 1,
  "WorkorderID": "MACHINE-01",
  "GoodCount": 120,
  "BadCount": 3,
  "Temperature": 42.7
}
```

---

## 3. Device Twin

Stan urządzenia synchronizowany jest przy użyciu **Device Twin**.

### Sekcja `reported` (zgłaszana przez urządzenie)

```json
"reported": {
  "productionRate": 85,
  "deviceErrors": []
}
```

- **productionRate** - aktualny poziom produkcji (%)  
- **deviceErrors** - lista aktywnych błędów

### Sekcja `desired` (oczekiwana konfiguracja z chmury)

Można ją edytować np. przez Azure IoT Explorer:

```json
"desired": {
  "productionRate": 90
}
```

---

## 4. Direct Methods (Metody Bezpośrednie)

Agent obsługuje zdalne metody wywoływane np. przez Azure IoT Explorer.  
Nie wymagają żadnego payloadu.

### Dostępne metody:

| Metoda | Działanie |
|--------|-----------|
| **EmergencyStop** | Natychmiastowe zatrzymanie połączonego urządzenia |
| **ResetErrorStatus** | Wyczyszczenie rejestru błędów |

Przykład wywołania:

```json
{
  "methodName": "EmergencyStop",
  "responseTimeoutInSeconds": 30
}
```

---

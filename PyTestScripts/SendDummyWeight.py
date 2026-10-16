import requests
import random
from datetime import datetime, timezone

API_URL = "https://localhost:7262/api/weight"
API_KEY = ""

headers = {
    "X-Api-Key": API_KEY,
    "Content-Type": "application/json"
}

measurement = {
    "measurementId": random.getrandbits(63),
    "measuredAt": datetime.now(timezone.utc).isoformat(),
    "weightKg": 116.8,
    "impedanceOhm": 359
}

try:
    response = requests.post(
        API_URL,
        headers=headers,
        json=measurement,
        verify=False   # ASP.NET development certificate
    )

    print(f"Status: {response.status_code}")
    print(f"Response: {response.text}")

except requests.RequestException as ex:
    print(f"Request failed: {ex}")
import requests
import random
from datetime import datetime, timezone

API_URL = "https://localhost:7262/api/weight"
API_KEY = "FCEB10384BA4F791FDB87B5659D486A8F323E5401E24473B75CB7518D3D77C4B"

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
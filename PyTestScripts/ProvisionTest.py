import requests
import urllib3

# Suppress warning for the ASP.NET development certificate.
urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

# Change port to match HealthDataCollector.
API_URL = "https://localhost:7262/api/provision"

payload = {
    "name": "Python Test Collector",
    "deviceType": "ESP32"
}

print(f"POST {API_URL}")
print(f"Request: {payload}")
print()

try:
    response = requests.post(
        API_URL,
        json=payload,
        timeout=5,
        verify=False
    )

    print(f"HTTP Status: {response.status_code}")

    if response.ok:
        data = response.json()

        print("Provisioning successful")
        print(f"Source ID:      {data.get('sourceId')}")
        print(f"API Key:        {data.get('apiKey')}")
        print(f"Correlation ID: {data.get('correlationId')}")
    else:
        print("Provisioning failed")
        print(response.text)

except requests.RequestException as ex:
    print(f"Request failed: {ex}")
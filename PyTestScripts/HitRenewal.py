import requests
import urllib3

# Suppress warning for local ASP.NET dev certificate
urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

API_URL = "https://localhost:7262/api/key/renew"
API_KEY = "01C2EBBDEB9EB945842D5209AFB919B159915C86D58BEE3ED6BF763B79191193"

headers = {
    "X-Api-Key": API_KEY
}

response = requests.post(
    API_URL,
    headers=headers,
    verify=False
)

print(f"Status: {response.status_code}")

try:
    data = response.json()
    print("Response:")
    print(data)

    if response.ok:
        print("\n--- NEW CREDENTIAL ---")
        print(f"Source ID:  {data.get('sourceId')}")
        print(f"API Key:    {data.get('apiKey')}")
        print(f"Expires At: {data.get('apiKeyExpiresAt')}")

except ValueError:
    print("Response:")
    print(response.text)
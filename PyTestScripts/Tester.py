import asyncio
import requests
import urllib3
import random
from ScaleSendAPI import listen_for_scale

from datetime import datetime, timezone

# Suppress warning for the ASP.NET development certificate.
urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

# Change port to match HealthDataCollector.
PROVISION_API_URL = "https://localhost:7262/api/provision"
RENEW_API_KEY_URL = "https://localhost:7262/api/key/renew"
SUBMIT_WEIGHT_API_URL = "https://localhost:7262/api/weight"

key_store = {}      

def print_keys():
    print ("Name:          API KEY:")
    for name, key in key_store.items():
        print(name, key)

def test_provision():
    print("Provisioning Test")
    deviceName = input("Device Name >")
    deviceType = input("Device Type >")

    payload = {
        "name": deviceName,
        "deviceType": deviceType
    }

    print(f"POST {PROVISION_API_URL}")
    print(f"Request: {payload}")
    print()

    try:
        response = requests.post(
            PROVISION_API_URL,
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

        saveChoice = input ("Save Key? y/n>")
        if saveChoice.strip().lower() == "y":
            key_store[deviceName[:12].lower()] = data.get('apiKey')

    except requests.RequestException as ex:
        print(f"Request failed: {ex}")
        input("Enter to continue")
        return
    
    return

def test_renew_key():
    print_keys()
    print("Enter keyname(max 12 characters) or paste API Key:")
    keyToUse = input(">").lower()
    if (len(keyToUse) <= 12):
        if (keyToUse.lower() in key_store):
            keyToUse = key_store[keyToUse]
        else:
            input("Key not found.")
            return

    headers = {
        "X-Api-Key": keyToUse,
    }

    try:
        response = requests.post(
        RENEW_API_KEY_URL,
        headers=headers,
        timeout=5,
        verify=False   # ASP.NET development certificate
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

        print(f"Status: {response.status_code}")
        print(f"Response: {response.text}")

        saveChoice = input ("Save new Key? y/n>")
        if saveChoice.strip().lower() == "y":
            key_store[keyToUse[:11].lower() + "R"] = data.get('apiKey')

    except requests.RequestException as ex:
        print(f"Request failed: {ex}")
        input("Enter to continue")
        return
    
    return

def test_dummy_weight():
    print_keys()
    print("Enter keyname(max 12 characters) or paste API Key:")
    keyToUse = input(">").lower()
    if (len(keyToUse) <= 12):
        if (keyToUse.lower() in key_store):
            keyToUse = key_store[keyToUse]
        else:
            input("Key not found.")
            return
        
    print (keyToUse)

    weight = float(input("Enter Weight: "))
    impedence = int(input("Enter Impedence: "))

    headers = {
        "X-Api-Key": keyToUse,
        "Content-Type": "application/json"
    }

    measurement = {
        "measurementId": random.getrandbits(63),
        "measuredAt": datetime.now(timezone.utc).isoformat(),
        "weightKg": weight,
        "impedanceOhm": impedence
    }

    try:
        response = requests.post(
            SUBMIT_WEIGHT_API_URL,
            headers=headers,
            json=measurement,
            verify=False   # ASP.NET development certificate
        )

        print(f"Status: {response.status_code}")
        print(f"Response: {response.text}")

    except requests.RequestException as ex:
        print(f"Request failed: {ex}")
        input("Enter to continue")
        return

    return

def run_live_bridge():
    print_keys()
    print("Enter keyname(max 12 characters) or paste API Key:")
    keyToUse = input(">").lower()
    if (len(keyToUse) <= 12):
        if (keyToUse.lower() in key_store):
            keyToUse = key_store[keyToUse]
        else:
            input("Key not found.")
            return
        
    asyncio.run(listen_for_scale(SUBMIT_WEIGHT_API_URL,keyToUse))
    return


while True:
    print("Health Data Test Package\n")
    print("1. Provision New Device")
    print("2. Renew API Key")
    print("3. Send test weight measurement")
    print("4. Setup Live bridge with scale")
    print("0. Quit")
    opt = int(input("\nOption > "))

    match opt:
        case 1:
            test_provision()
        case 2:
            test_renew_key()
        case 3:
            test_dummy_weight()
        case 4:
            run_live_bridge()
        case 0:
            break       #Exit
        case _:
            print("Invalid option")


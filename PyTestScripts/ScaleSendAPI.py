import asyncio
import random
import requests
import urllib3

from datetime import datetime
from bleak import BleakScanner

urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

SCALE_SERVICE = "0000181b-0000-1000-8000-00805f9b34fb"

# CHANGE THIS TO THE ACTUAL ADDRESS SHOWN BY ASP.NET
API_URL = "https://localhost:7262/api/weight"
API_KEY = "ENTER_KEY_HERE"

last_data = None
last_sent_measurement = None


def generate_measurement_id():
    # Keep within signed 64-bit range for now.
    return random.getrandbits(63)


def send_measurement(weight_kg, impedance_ohm, measured_at, api_url, api_key):
    measurement_id = generate_measurement_id()

    headers = {
        "X-Api-Key": api_key,
        "Content-Type": "application/json"
    }

    payload = {
        "measurementId": measurement_id,
        "measuredAt": measured_at.isoformat(),
        "weightKg": weight_kg,
        "impedanceOhm": impedance_ohm
    }

    print()
    print("POSTING TO API")
    print(f"URL:     {api_url}")
    print(f"Payload: {payload}")

    try:
        response = requests.post(
            API_URL,
            headers=headers,
            json=payload,
            timeout=5,
            verify=False
        )

        print(f"Status:  {response.status_code}")
        print(f"Response: {response.text}")

    except requests.RequestException as ex:
        print(f"API REQUEST FAILED: {ex}")

    print()


def callback(device, adv, api_url, api_key):
    global last_data
    global last_sent_measurement

    data = adv.service_data.get(SCALE_SERVICE)

    if data is None:
        return

    if len(data) != 13:
        print(f"Unexpected packet length: {len(data)}")
        print(data.hex("-").upper())
        return

    if data == last_data:
        return

    last_data = bytes(data)

    # ----- raw packet -----

    print()
    print("RAW:", data.hex("-").upper())

    flag0 = data[0]
    flag1 = data[1]

    # ----- scale timestamp -----

    year = int.from_bytes(data[2:4], byteorder="little")
    month = data[4]
    day = data[5]
    hour = data[6]
    minute = data[7]
    second = data[8]

    try:
        measured_at = datetime(
            year,
            month,
            day,
            hour,
            minute,
            second
        )
    except ValueError:
        measured_at = None

    # ----- impedance -----

    raw_impedance = int.from_bytes(
        data[9:11],
        byteorder="little",
        signed=False
    )

    if raw_impedance in (0x0000, 0xFFFF, 0xFFFE):
        impedance_ohm = None
    else:
        impedance_ohm = raw_impedance

    # ----- weight -----

    raw_weight = int.from_bytes(
        data[11:13],
        byteorder="little",
        signed=False
    )

    weight_kg = raw_weight / 200.0

    # ----- display -----

    impedance_text = (
        f"{impedance_ohm} ohm"
        if impedance_ohm is not None
        else "---"
    )

    timestamp_text = (
        measured_at.isoformat()
        if measured_at is not None
        else "INVALID TIME"
    )

    print(
        f"Weight:     {weight_kg:.2f} kg\n"
        f"Impedance:  {impedance_text}\n"
        f"Raw Imp:    0x{raw_impedance:04X}\n"
        f"Flags:      {flag0:02X}-{flag1:02X}\n"
        f"Scale Time: {timestamp_text}"
    )

    # -------------------------------------------------------
    # 02-26 = valid weight + impedance state we've observed
    # -------------------------------------------------------

    if flag0 == 0x02 and flag1 == 0x26:

        if impedance_ohm is None:
            print("02-26 packet had no usable impedance -- not sending.")
            return

        if measured_at is None:
            print("Invalid scale timestamp -- not sending.")
            return

        # Avoid sending the same physical reading twice
        fingerprint = (
            raw_weight,
            raw_impedance,
            year,
            month,
            day,
            hour,
            minute,
            second
        )

        if fingerprint == last_sent_measurement:
            print("Already sent this measurement.")
            return

        last_sent_measurement = fingerprint

        print("*** ACCEPTED MEASUREMENT ***")

        send_measurement(
            weight_kg,
            impedance_ohm,
            measured_at,
            api_url, 
            api_key
        )


async def listen_for_scale(api_url, api_key):

    def scanner_callback(device, adv):
        callback(device, adv, api_url, api_key)

    scanner = BleakScanner(scanner_callback)

    await scanner.start()

    print("Listening for Xiaomi scale...")
    print("Ctrl+C to stop.")

    try:
        while True:
            await asyncio.sleep(1)

    except KeyboardInterrupt:
        pass

    finally:
        await scanner.stop()


if __name__ == "__main__":
    asyncio.run(listen_for_scale())
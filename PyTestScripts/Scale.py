import asyncio
from bleak import BleakScanner

SCALE_SERVICE = "0000181b-0000-1000-8000-00805f9b34fb"

last_data = None


def callback(device, adv):
    global last_data

    data = adv.service_data.get(SCALE_SERVICE)

    if not data or len(data) != 13:
        return

    if data == last_data:
        return

    last_data = data

    # First two bytes are control/status flags
    control = int.from_bytes(data[0:2], "little")

    # Timestamp
    year = int.from_bytes(data[2:4], "little")
    month = data[4]
    day = data[5]
    hour = data[6]
    minute = data[7]
    second = data[8]

    # Measurement fields
    raw_impedance = int.from_bytes(data[9:11], "little")
    raw_weight = int.from_bytes(data[11:13], "little")

    weight_kg = raw_weight / 200.0

    # Stable flag
    stable = bool(control & (1 << 10))

    # Xiaomi appears to use these as invalid/no-reading values
    has_impedance = raw_impedance not in (
        0x0000,
        0xFFFF,
        0xFFFE,
    )

    impedance_text = (
        f"{raw_impedance} ohm"
        if has_impedance
        else "---"
    )

    print(
        f"{weight_kg:6.2f} kg | "
        f"stable={stable:<5} | "
        f"impedance={impedance_text:<10} | "
        f"flags={data[0]:02X}-{data[1]:02X} | "
        f"{year:04d}-{month:02d}-{day:02d} "
        f"{hour:02d}:{minute:02d}:{second:02d}"
    )


async def main():
    scanner = BleakScanner(callback)

    await scanner.start()

    print("Listening for Xiaomi scale... Ctrl+C to stop.")

    try:
        while True:
            await asyncio.sleep(1)
    except KeyboardInterrupt:
        pass
    finally:
        await scanner.stop()


if __name__ == "__main__":
    asyncio.run(main())
import asyncio
from datetime import datetime
from bleak import BleakScanner, BleakClient

TIME_UUID = "00002a2b-0000-1000-8000-00805f9b34fb"

async def main():
    print("Looking for MIBFS...")

    device = await BleakScanner.find_device_by_filter(
        lambda d, adv: d.name == "MIBFS"
    )

    if not device:
        print("Scale not found. Wake it up and try again.")
        return

    print(f"Found {device.name}: {device.address}")

    async with BleakClient(device) as client:
        print("Connected.")

        now = datetime.now()

        # BLE Current Time characteristic:
        # year (LE), month, day, hour, minute, second,
        # day-of-week, fractions256, adjust-reason

        day_of_week = now.isoweekday()  # Monday=1 ... Sunday=7

        payload = bytes([
            now.year & 0xFF,
            (now.year >> 8) & 0xFF,
            now.month,
            now.day,
            now.hour,
            now.minute,
            now.second,
            day_of_week,
            0x00,       # fractions256
            0x01        # adjust reason: manual time update
        ])

        print("Writing:", payload.hex("-").upper())

        await client.write_gatt_char(
            TIME_UUID,
            payload,
            response=True
        )

        print("Time written.")

        # Read it back
        value = await client.read_gatt_char(TIME_UUID)

        print("Read back:", value.hex("-").upper())


asyncio.run(main())
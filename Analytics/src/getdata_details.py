import requests
import json
import os

FIREBASE_BASE_URL = "https://portalmario-cs526-default-rtdb.firebaseio.com"
EXPORT_DIR = "Analytics/Beta_Data/Beta_Details"

SECTIONS = [
    ("MainMenu", "MainMenu"),
    ("level_-1", "level_-1"),
    ("level_0", "level_0"),
    ("level_1", "level_1"),
    ("level_2", "level_2")
]

os.makedirs(EXPORT_DIR, exist_ok=True)

def fetch_all_test_data():
    url = f"{FIREBASE_BASE_URL}/test.json"
    response = requests.get(url)
    if response.status_code == 200:
        return response.json()

def export_by_section(data):
    for firebase_key, filename in SECTIONS:
        section_data = {}

        for player_id, player_data in data.items():
            if firebase_key in player_data:
                section_data[player_id] = player_data[firebase_key]

        filepath = os.path.join(EXPORT_DIR, f"{filename}.json")
        with open(filepath, 'w') as f:
            json.dump(section_data, f, indent=2)

        print(f"Exported {filename}.json with {len(section_data)} players")

if __name__ == "__main__":
    data = fetch_all_test_data()
    if data:
        export_by_section(data)
from datetime import datetime
import json
import os
import re
import zipfile


def extract_timestamps(obj):
    """Parcourt récursivement les objets JSON pour trouver les horodatages (ms ou s)."""
    ts_list = []
    if isinstance(obj, dict):
        for k, v in obj.items():
            if isinstance(v, (int, float)):
                if any(
                    term in k.lower()
                    for term in ['millis', 'timestamp', 'time', 'date']
                ):
                    if 946684800000 < v < 1800000000000:  # ms (2000-2027)
                        ts_list.append(v / 1000.0)
                    elif 946684800 < v < 1800000000:  # secondes
                        ts_list.append(v)
            elif isinstance(v, str):
                if v.isdigit():
                    val = float(v)
                    if 946684800000 < val < 1800000000000:
                        ts_list.append(val / 1000.0)
                    elif 946684800 < val < 1800000000:
                        ts_list.append(val)
                else:
                    try:
                        dt = datetime.fromisoformat(v.replace('Z', '+00:00'))
                        ts_list.append(dt.timestamp())
                    except Exception:
                        pass
            elif isinstance(v, (dict, list)):
                ts_list.extend(extract_timestamps(v))
    elif isinstance(obj, list):
        for item in obj:
            ts_list.extend(extract_timestamps(item))
    return ts_list


def parse_takeout_zip(zip_path, output_csv_path='jeux_takeout.csv'):
    games = {}

    with zipfile.ZipFile(zip_path, 'r') as z:
        for f in z.namelist():
            # Détection des dossiers de jeux dans Google Play Games
            if 'Services de jeux Google' in f and '/Jeux/' in f:
                parts = re.split(
                    r'Services de jeux Google[\s\xa0]Play/Jeux/', f
                )
                if len(parts) > 1 and parts[1]:
                    path_parts = parts[1].split('/')
                    game_name = path_parts[0]
                    if not game_name:
                        continue

                    if game_name not in games:
                        games[game_name] = {'max_ts': None}

                    if f.endswith('.json'):
                        try:
                            data = json.loads(
                                z.read(f).decode('utf-8', errors='ignore')
                            )
                            timestamps = extract_timestamps(data)
                            for ts in timestamps:
                                if (
                                    games[game_name]['max_ts'] is None
                                    or ts > games[game_name]['max_ts']
                                ):
                                    games[game_name]['max_ts'] = ts
                        except Exception:
                            pass

    # Structuration du résultat au format CSV
    rows = []
    for gname, info in games.items():
        if info['max_ts']:
            date_str = datetime.fromtimestamp(info['max_ts']).strftime(
                '%Y-%m-%d'
            )
        else:
            date_str = 'N/A'

        # Formatage du nom de package
        slug = re.sub(r'[^a-zA-Z0-9]', '', gname).lower()
        package_name = f'com.game.{slug}'

        # Nettoyage du titre pour affichage
        clean_name = gname.replace('_', ': ')

        rows.append((package_name, clean_name, date_str))

    # Tri par date de dernière utilisation (du plus récent au plus ancien)
    rows.sort(key=lambda x: x[2], reverse=True)

    # Écriture du fichier CSV
    with open(output_csv_path, 'w', encoding='utf-8') as out:
        out.write('package,name,last_time_used\n')
        for pkg, name, dt in rows:
            out.write(f'{pkg},{name},{dt}\n')

    print(
        f'Analyse terminée : {len(rows)} jeux exportés dans "{output_csv_path}".'
    )


# Renseigne ici le chemin exact de ton fichier .zip Takeout
zip_filename = "C:\\Users\\Yann\\Downloads\\takeout-20260930T130711Z-1-001.zip"
parse_takeout_zip(zip_filename)
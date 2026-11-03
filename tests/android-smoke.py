"""Read-only navigation smoke test on an Android emulator; no account is created."""
import re
import subprocess
import time
import xml.etree.ElementTree as ET
from pathlib import Path

output = Path("artifacts/android-review")
output.mkdir(parents=True, exist_ok=True)
package = "br.org.coracaoanimal.app"

def adb(*args):
    return subprocess.check_output(["adb", *args])

def tree():
    adb("shell", "uiautomator", "dump", "/sdcard/coracao-ui.xml")
    return ET.fromstring(adb("shell", "cat", "/sdcard/coracao-ui.xml"))

def find(text, timeout=75):
    deadline = time.monotonic() + timeout
    while time.monotonic() < deadline:
        for node in tree().iter("node"):
            if node.get("text") == text or node.get("content-desc", "").split(",")[0] == text:
                return node
        if not adb("shell", "pidof", package).strip():
            raise RuntimeError("Android app exited unexpectedly")
        time.sleep(1)
    raise AssertionError(f"Android UI did not show {text!r}")

def tap(text):
    node = find(text)
    x1, y1, x2, y2 = map(int, re.findall(r"\d+", node.attrib["bounds"]))
    adb("shell", "input", "tap", str((x1+x2)//2), str((y1+y2)//2))

def screenshot(name):
    (output / f"{name}.png").write_bytes(adb("exec-out", "screencap", "-p"))
    (output / f"{name}.xml").write_bytes(ET.tostring(tree()))

apks = sorted(Path("src/CoracaoAnimal.App/bin/Release/net10.0-android").glob("*Signed.apk"))
assert apks, "Signed APK missing"
adb("install", "-r", str(apks[0]))
adb("shell", "monkey", "-p", package, "-c", "android.intent.category.LAUNCHER", "1")
find("Buscar animais")
screenshot("01-explorar")
tap("Ajudar")
find("Cada gesto faz diferença")
screenshot("02-ajudar")
tap("Conta")
find("Bem-vindo de volta")
screenshot("03-entrar")
tap("Criar conta")
find("Vamos começar?")
screenshot("04-cadastro")
assert adb("shell", "pidof", package).strip()
print("PASS: Android app starts and navigation, help and account forms remain accessible.")

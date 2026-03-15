# Survival-Horror-Flashlight-System
Survival Horror Flashlight System
Bu modül, gerilim ve hayatta kalma oyunları için tasarlanmış gelişmiş bir el feneri ve batarya yönetim sistemidir.

Özellikler:

Batarya Yönetimi & Dinamik Glitch: Fenerin pili azaldığında ışık otomatik olarak pırpır etmeye (flicker) başlar ve glitch olaylarını (event) tetikler. Pil değiştirildiğinde (Reload) eski gücüne döner.

Raycast Tabanlı Sersemletme (Stun): Fener ışığı belirli bir mesafedeki düşmanlara (LayerMask ile filtrelenmiş) tutulduğunda pili daha hızlı tüketerek onları sersemletme mekaniğini destekler.

Event-Driven Mimari: UnityEvent kullanımı sayesinde Ses (Audio), UI Bildirimleri (Notifications), Görev (Task) ve Kayıt (Save) sistemlerine tek satır kod yazmadan Inspector üzerinden entegre edilebilir. Tuş atamaları (Toggle/Reload) tamamen modülerdir.

Kurulum:

Fener modelinize FlashlightController ve FlashlightCollectable scriptlerini atayın.

Inspector'dan On Flashlight Glitch, On Toggle gibi event'lere kendi ses yöneticinizi (Örn: VoiceManager.PlayFlashlightGlitch) bağlayın.

On Flashlight Collected event'ine Save ve Task yöneticilerinizin ilgili fonksiyonlarını sürükleyip bırakarak sistemi oyuna dahil edin.

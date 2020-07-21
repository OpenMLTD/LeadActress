# LeadActress

A Unity project that reproduces certain degree of MLTD (MiriShita/[ミリシタ](https://millionlive.idolmaster.jp/theaterdays/)) stage performance.

What it does:

- serve as experiments and output data references of MillionDance in [MLTDTools](https://github.com/OpenMLTD/MLTDTools).

What it does not:

- target end users;
- aim to reproduce the full MV setup.

## Requirements

The project is created using Unity 2019.4 Personal. Earlier versions may work but there may be issues with dependency management.

## Usage

First restore the packages. Usually the Editor will do it for you.

There are some things you need before launching:

- Idol models (`ch_aaabbb_ccc.unity3d` and `cb_aaabbb_ccc.unity3d`). Note that the head and body models can come from different costumes.
- Stage scene (`stagezzz.unity3d` and associated prop scenes).
- Dance animation (`dan_xxxxxx_yy.imo.unity3d`).
- Camera animation (`cam_xxxxxx.imo.unity3d`).
- Scenario (`scrobj_xxxxxx.unity3d`).
- Music (`song3_xxxxxx.unity3d`). Decode the audio as MP3 and keep the name (`song3_xxxxxx.mp3`).

Place the files above in [streaming assets folder](https://docs.unity3d.com/Manual/StreamingAssets.html). For Unity Editor it's the `Assets/StreamingAssets` folder.

Then, set corresponding properties on the components in the scene. The default scene already included an example.

[Demo](https://www.bilibili.com/video/BV1Vf4y1R7Us/)

## Credits

- [Santarh/MToon](https://github.com/Santarh/MToon)
- [naudio/NLayer](https://github.com/naudio/NLayer)
- [Unlit shadowcaster](https://styly.cc/tips/unlitcastshadow-go-shader/)

## License

BSD 3-Clause Clear License

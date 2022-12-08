namespace Yurowm {
    public static class VibratorUtils {
    
        public static void VibrateWithPower(float power) {
            Vibrator.AndroidVibrate(.02f);
            
            Vibrator.iOSVibrate(power <.7f ? Vibrator.iOSVibrateType.Pop : Vibrator.iOSVibrateType.Peek);
        }
    }
}
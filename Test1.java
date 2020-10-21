package jp.co.inc.meida.video.frame;

import java.nio.charset.Charset;
import java.nio.charset.StandardCharsets;

public class Test1 {

	public static void main(String[] args) {
		Charset charset = StandardCharsets.UTF_8;

        // 送信先URL
        String strPostUrl = "http://httpbin.org/post";
        // アカウント情報のJSON文字列
        String JSON = "{\"userid\":\"taro\", \"password\":\"123\"}";
        // 認証
        HttpSendJSON httpSendJSON = new HttpSendJSON();
        String result = httpSendJSON.callPost(strPostUrl, JSON);

        // 結果の表示
        System.out.println(result);
	}

}

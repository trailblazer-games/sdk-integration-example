#import <UIKit/UIKit.h>
#import <WebKit/WebKit.h>

@interface TreasurePlayWebView : NSObject
@property (nonatomic, strong) WKWebView *webView;
@property (nonatomic, strong) UIViewController *presentingViewController;
@end

@implementation TreasurePlayWebView

+ (instancetype)sharedInstance {
    static TreasurePlayWebView *sharedInstance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        sharedInstance = [[self alloc] init];
    });
    return sharedInstance;
}

- (instancetype)init {
    self = [super init];
    if (self) {
        [self setupWebView];
    }
    return self;
}

- (void)setupWebView {
    WKWebViewConfiguration *config = [[WKWebViewConfiguration alloc] init];
    config.allowsInlineMediaPlayback = YES;
    config.mediaTypesRequiringUserActionForPlayback = WKAudiovisualMediaTypeNone;
    
    // Enable mobile viewport behavior (mimics Chrome DevTools device mode)
    WKPreferences *preferences = [[WKPreferences alloc] init];
    preferences.javaScriptEnabled = YES;
    config.preferences = preferences;
    
    self.webView = [[WKWebView alloc] initWithFrame:CGRectZero configuration:config];
    self.webView.translatesAutoresizingMaskIntoConstraints = NO;
    self.webView.backgroundColor = [UIColor whiteColor];
    
    // Enable scrolling and scaling for proper mobile rendering
    self.webView.scrollView.scrollEnabled = YES;
    self.webView.scrollView.bounces = YES;
    
    // Add close button
    UIButton *closeButton = [UIButton buttonWithType:UIButtonTypeSystem];
    [closeButton setTitle:@"Close" forState:UIControlStateNormal];
    [closeButton addTarget:self action:@selector(closeWebView) forControlEvents:UIControlEventTouchUpInside];
    closeButton.translatesAutoresizingMaskIntoConstraints = NO;
    [self.webView addSubview:closeButton];
    
    // Set up constraints
    [NSLayoutConstraint activateConstraints:@[
        [closeButton.topAnchor constraintEqualToAnchor:self.webView.topAnchor constant:20],
        [closeButton.trailingAnchor constraintEqualToAnchor:self.webView.trailingAnchor constant:-20],
        [closeButton.widthAnchor constraintEqualToConstant:60],
        [closeButton.heightAnchor constraintEqualToConstant:30]
    ]];
}

- (void)showWebViewWithURL:(NSString *)url forceRefresh:(BOOL)forceRefresh {
    dispatch_async(dispatch_get_main_queue(), ^{
        // Check if we should clear storage
        NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
        BOOL isFirstLaunch = ![defaults boolForKey:@"TreasurePlayWebView_FirstLaunchComplete"];
        BOOL shouldClearStorage = isFirstLaunch || forceRefresh;
        
        if (shouldClearStorage) {
            if (forceRefresh) {
                NSLog(@"[TreasurePlaySDK] Force refresh requested - clearing storage");
            } else {
                NSLog(@"[TreasurePlaySDK] First WebView launch - clearing storage for fresh auth");
            }
            
            // Clear all website data (localStorage, cookies, cache)
            NSSet *websiteDataTypes = [NSSet setWithArray:@[
                WKWebsiteDataTypeDiskCache,
                WKWebsiteDataTypeMemoryCache,
                WKWebsiteDataTypeLocalStorage,
                WKWebsiteDataTypeCookies,
                WKWebsiteDataTypeSessionStorage,
                WKWebsiteDataTypeIndexedDBDatabases,
                WKWebsiteDataTypeWebSQLDatabases
            ]];
            
            NSDate *dateFrom = [NSDate dateWithTimeIntervalSince1970:0];
            [[WKWebsiteDataStore defaultDataStore] removeDataOfTypes:websiteDataTypes
                                                       modifiedSince:dateFrom
                                                   completionHandler:^{
                NSLog(@"[TreasurePlaySDK] Cleared all website data");
                
                // Mark as launched (only if it's the first launch, not on forceRefresh)
                if (isFirstLaunch) {
                    [defaults setBool:YES forKey:@"TreasurePlayWebView_FirstLaunchComplete"];
                    [defaults synchronize];
                }
                
                [self presentWebViewWithURL:url];
            }];
        } else {
            NSLog(@"[TreasurePlaySDK] Subsequent WebView launch - preserving session");
            [self presentWebViewWithURL:url];
        }
    });
}

- (void)presentWebViewWithURL:(NSString *)url {
    UIViewController *rootViewController = [UIApplication sharedApplication].keyWindow.rootViewController;
    
    // Find the topmost presented view controller
    while (rootViewController.presentedViewController) {
        rootViewController = rootViewController.presentedViewController;
    }
    
    self.presentingViewController = rootViewController;
    
    // Create a container view controller
    UIViewController *containerVC = [[UIViewController alloc] init];
    containerVC.view.backgroundColor = [UIColor whiteColor];
    containerVC.modalPresentationStyle = UIModalPresentationFullScreen;
    
    // Add webview to container
    [containerVC.view addSubview:self.webView];
    [NSLayoutConstraint activateConstraints:@[
        [self.webView.topAnchor constraintEqualToAnchor:containerVC.view.safeAreaLayoutGuide.topAnchor],
        [self.webView.leadingAnchor constraintEqualToAnchor:containerVC.view.leadingAnchor],
        [self.webView.trailingAnchor constraintEqualToAnchor:containerVC.view.trailingAnchor],
        [self.webView.bottomAnchor constraintEqualToAnchor:containerVC.view.bottomAnchor]
    ]];
    
    // Load URL with custom User-Agent
    NSURL *nsurl = [NSURL URLWithString:url];
    NSMutableURLRequest *request = [NSMutableURLRequest requestWithURL:nsurl];
    
    // Set mobile User-Agent for iOS
    NSString *userAgent = [NSString stringWithFormat:@"Mozilla/5.0 (iPhone; CPU iPhone OS %@ like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148",
                          [[UIDevice currentDevice] systemVersion]];
    [request setValue:userAgent forHTTPHeaderField:@"User-Agent"];
    
    NSLog(@"[TreasurePlaySDK] WebView User-Agent: %@", userAgent);
    
    [self.webView loadRequest:request];
    
    // Present the webview
    [rootViewController presentViewController:containerVC animated:YES completion:nil];
}

- (void)hideWebView {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (self.presentingViewController) {
            [self.presentingViewController dismissViewControllerAnimated:YES completion:nil];
            self.presentingViewController = nil;
        }
    });
}

- (void)closeWebView {
    [self hideWebView];
}

- (BOOL)isWebViewVisible {
    return self.presentingViewController != nil;
}

@end

// C interface for Unity
extern "C" {
    void _TreasurePlay_ShowWebView(const char* url, bool forceRefresh) {
        NSString *nsurl = [NSString stringWithUTF8String:url];
        [[TreasurePlayWebView sharedInstance] showWebViewWithURL:nsurl forceRefresh:forceRefresh];
    }
    
    void _TreasurePlay_HideWebView() {
        [[TreasurePlayWebView sharedInstance] hideWebView];
    }
    
    bool _TreasurePlay_IsWebViewVisible() {
        return [[TreasurePlayWebView sharedInstance] isWebViewVisible];
    }
}

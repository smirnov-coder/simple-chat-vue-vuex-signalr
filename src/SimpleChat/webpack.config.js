"use strict";

const path = require("path");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const OptimizeCSSAssetsPlugin = require("optimize-css-assets-webpack-plugin");
const VueLoaderPlugin = require("vue-loader/lib/plugin");
const { CleanWebpackPlugin } = require("clean-webpack-plugin");
const CopyWebpackPlugin = require("copy-webpack-plugin");
const BundleAnalyzerPlugin = require("webpack-bundle-analyzer").BundleAnalyzerPlugin;
const UglifyJSPlugin = require("uglifyjs-webpack-plugin");

module.exports = () => {
    const isDevBuild = !(process.env.NODE_ENV && process.env.NODE_ENV === "production");
    return [{
        mode: isDevBuild ? "development" : "production",
        devtool: isDevBuild ? "#eval" : "",
        stats: {
            modules: false
        },
        entry: {
            app: "./ClientApp/main.js"
        },
        resolve: {
            extensions: ["*", ".js", ".vue"],
            alias: {
                "vue$": "vue/dist/vue.esm.js",
                "@": path.resolve(__dirname, "ClientApp")
            },
       },
        output: {
            path: path.join(__dirname, "wwwroot"),
            filename: `js/[name].bundle${isDevBuild ? "" : ".min"}.js`,
            publicPath: "/"
        },
        module: {
            rules: [
                {
                    test: /\.vue$/,
                    include: path.resolve(__dirname, "ClientApp"),
                    loader: "vue-loader",
                    //options: {
                    //    loaders: {
                    //        js: "babel-loader"
                    //    }
                    //}
                },
                {
                    test: /\.js$/,
                    include: path.resolve(__dirname, "ClientApp"),
                    loader: "babel-loader?cacheDirectory=true",
                    //options: {
                    //    presets: ["@babel/preset-env"]
                    //}
                },
                {
                    test: /\.css$/,
                    use: [isDevBuild ? "vue-style-loader" : MiniCssExtractPlugin.loader, "css-loader"]
                },
                {
                    test: /\.(woff2?|ttf|otf|eot|svg)$/,
                    use: {
                        loader: "file-loader",
                        options: { name: "fonts/[name].[ext]" }
                    }
                }
            ]
        },
        plugins: [
            new VueLoaderPlugin(),
            new CleanWebpackPlugin({
                verbose: true
            }),
            new CopyWebpackPlugin([
                {
                    from: "./ClientApp/assets/favicon",
                    to: "./"
                }
            ]),
        ].concat(isDevBuild ? [] : [
            new MiniCssExtractPlugin({
                filename: "css/[name].bundle.min.css"
            }),
            new OptimizeCSSAssetsPlugin({
                cssProcessorPluginOptions: {
                    preset: [
                        "default",
                        { discardComments: { removeAll: true } }
                    ],
                }
            }),
            new UglifyJSPlugin({
                cache: true,
                parallel: true
            }),
            new BundleAnalyzerPlugin({
                analyzerMode: "static",
                reportFilename: "bundle-report.html",
                openAnalyzer: false
            }),
        ]),
        optimization: {
            //concatenateModules: true,
            splitChunks: {
                chunks: "all",
                minSize: 0,
                cacheGroups: {
                    vendor: {
                        test: /[\\/](node_modules|ClientApp[\\/]assets[\\/]lib)[\\/]/,
                        name: "vendor",
                        filename: `js/[name].bundle${isDevBuild ? "" : ".min"}.js`
                    }
                }
            }
        },
    }];
}

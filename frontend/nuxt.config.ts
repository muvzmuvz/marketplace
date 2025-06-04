// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2024-11-01',
  devtools: { enabled: true },

  modules: ['@nuxtjs/tailwindcss', 'shadcn-nuxt'],

  routeRules: {
    '/uploads/**': { static: true }
  },

  shadcn: {
    /**
     * Prefix for all the imported component
     */
    prefix: '',
    /**
     * Directory that the component lives in.
     * @default "./components/ui"
     */
    componentDir: './components/ui'
  },

 
  runtimeConfig: {
    public: {

      apiBaseUrl: 'http://37.252.19.136:8080', 
      apiImageUrl: 'http://37.252.19.136:3000'
    }
  }
})

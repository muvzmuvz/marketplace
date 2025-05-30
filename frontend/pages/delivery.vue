<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import '@/assets/css/profile.css'

const router = useRouter()
const orders = ref([])
const isLoading = ref(true)
const fetchOrders = async () => {
  try {
    const response = await fetch('http://localhost:8080/order/orders', {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include'
    })

    if (!response.ok) throw new Error('Ошибка при загрузке заказов')

    const data = await response.json()

    // 📌 Сортируем от самого нового к старому по id
    orders.value = data.sort((a, b) => b.id - a.id)

  } catch (error) {
    console.error(error)
    router.push('/auth/login')
  } finally {
    isLoading.value = false
  }
}

onMounted(fetchOrders)
</script>

<template>
    <NavMenu />
    <div class="max-w l mx-auto p-6 space-y-6 container">
        <h1 class="text-3xl font-bold text-center">Мои заказы</h1>

        <Card v-if="isLoading">
            <CardContent class="p-6 space-y-4">
                <Skeleton class="h-6 w-3/4" />
                <Skeleton class="h-4 w-1/2" />
            </CardContent>
        </Card>

        <Card v-else>
            <div v-if="orders.length === 0" class="text-center text-gray-500">
                У вас нет заказов
            </div>

<div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
  <Card
    v-for="order in orders"
    :key="order.id"
    class="p-4 rounded-2xl shadow-sm bg-white hover:shadow-md transition h-full"
  >
    <p class="text-base font-semibold mb-1">Заказ №{{ order.id }}</p>
    <p class="text-sm text-gray-600 mb-2">
      Сумма: <span class="font-medium">{{ order.totalPrice }} ₽</span>
    </p>
    <p class="text-sm mb-3">
      Статус:
      <span
        :class="order.status === 0 ? 'text-yellow-500' : 'text-green-600'"
        class="font-medium"
      >
        {{ order.status === 0 ? 'В пути' : 'Завершен' }}
      </span>
    </p>

    <!-- Ограничение высоты для компактности -->
    <div class="space-y-3 max-h-36 overflow-y-auto pr-1">
      <div
        v-for="product in order.products"
        :key="product.productId"
        class="flex items-center gap-3"
      >
        <router-link :to="`/product/${product.productId}`">
          <img
            :src="product.product.images[0].path"
            alt="product.product.name"
            class="w-16 h-16 object-cover rounded-md border"
          />
        </router-link>
        <div class="text-sm">
          <p class="font-medium truncate">{{ product.product.name }}</p>
          <p class="text-gray-500">Кол-во: {{ product.quantity }} шт.</p>
        </div>
      </div>
    </div>
  </Card>
</div>
        </Card>
    </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import '@/assets/css/profile.css'
import ProductViewHistory from '@/components/ProductViewHistory/ProductViewHistory.vue'

const router = useRouter()
const user = ref(null)
const isLoading = ref(true)

const getStatusText = (status) => {
  switch (status) {
    case 0:
      return 'В сборке'
    case 1:
      return 'В пути'
    case 2:
      return 'Завершён'
    default:
      return 'Неизвестно'
  }
}

const getStatusClass = (status) => {
  switch (status) {
    case 0:
      return 'text-yellow-500'
    case 1:
      return 'text-blue-500'
    case 2:
      return 'text-green-600'
    default:
      return 'text-gray-400'
  }
}


const fetchProfile = async () => {
    try {
        const response = await fetch('http://localhost:8080/user/user', {
            method: 'GET',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include'
        })

        if (!response.ok) throw new Error('Не авторизован')

        user.value = await response.json()

        const ordersResponse = await fetch('http://localhost:8080/order/orders', {
            method: 'GET',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include'
        })

        if (ordersResponse.ok) {
            const ordersData = await ordersResponse.json()

            // 📌 Сортируем заказы от новых к старым по id
            user.value.orders = ordersData.sort((a, b) => b.id - a.id)
        } else {
            throw new Error('Ошибка при загрузке заказов')
        }

    } catch (error) {
        router.push('/auth/login')
    } finally {
        isLoading.value = false
    }
}

onMounted(() => {
    fetchProfile()
})

const logout = async () => {
    try {
        await fetch('http://localhost:8080/authuser/logout', {
            method: 'POST',
            credentials: 'include',
        })
    } catch (error) {
        console.error('Ошибка выхода:', error)
    }
    router.push('/auth/login')
}

const viewAllOrders = () => {
    router.push('/delivery') // Перенаправляем на страницу с полным списком заказов
}
</script>

<template>
    <NavMenu />
    <div class="max-w-4xl mx-auto p-6 space-y-6 container">
        
        <h1 class="text-3xl font-bold text-center">Личный кабинет</h1>

        <Card v-if="isLoading">
            <CardContent class="p-6 space-y-4">
                <Skeleton class="h-6 w-3/4" />
                <Skeleton class="h-4 w-1/2" />
            </CardContent>
        </Card>

        <Card v-else-if="user">
            <CardHeader>
                <CardTitle>Добро пожаловать, {{ user.name }}</CardTitle>
            </CardHeader>
            <CardContent>
                <p><strong>Email:</strong> {{ user.email }}</p>
                <div class="flex gap-4 mt-4">
                    <Button @click="logout">Выйти</Button>
                    <Button v-if="user.role === 2" @click="router.push('/seller-dashboard')" variant="outline">Управление магазином</Button>
                    <Button v-if="user.role === 0" @click="router.push('/admin-panel')" variant="destructive">Панель администратора</Button>
                </div>
            </CardContent>
        </Card>


        <!-- Заказы -->
        <div class="orders mt-6 ">
            <h2 class="text-xl font-semibold padding">Мои последние заказы</h2>

            <div v-if="!user?.orders || user.orders.length === 0" class="text-gray-500">У вас пока нет заказов</div>

            <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-4">
  <Card
    v-for="order in user.orders.slice(0, 2)"
    :key="order.id"
    class="p-4 space-y-2 rounded-2xl shadow-md border border-gray-200"
  >
    <p class="font-semibold text-lg">Заказ #{{ order.id }}</p>
    <p class="text-sm">Сумма: <span class="font-medium">{{ order.totalPrice }} ₽</span></p>
<p class="text-sm">
  Статус:
  <span :class="[getStatusClass(order.status), 'font-medium']">
    {{ getStatusText(order.status) }}
  </span>
</p>

    <!-- Товары -->
    <div class="mt-2">
      <p class="font-medium mb-1">Товары:</p>
      <div class="flex gap-2 overflow-x-auto max-w-full pb-2">
        <div
          v-for="product in order.products"
          :key="product.productId"
          class="flex-shrink-0 w-28"
        >
          <router-link :to="`/product/${product.productId}`" class="block text-center">
            <img
              :src="product.product.images[0].path"
              alt="product.name"
              class="w-24 h-24 object-cover mx-auto rounded-lg"
            />
            <p class="text-xs mt-1 truncate">{{ product.product.name }}</p>
            <p class="text-xs text-gray-500">Кол-во: {{ product.quantity }} шт.</p>
          </router-link>
        </div>
      </div>
    </div>
  </Card>
</div>

            <!-- Кнопка для просмотра всех заказов -->
            <div v-if="user?.orders.length > 2" class="text-center mt-4">
                <Button @click="viewAllOrders" variant="outline">Посмотреть все заказы</Button>
            </div>
            <ProductViewHistory :currentProductId="Number($route.params.id)" />
        </div>
        
        
    </div>
</template>
